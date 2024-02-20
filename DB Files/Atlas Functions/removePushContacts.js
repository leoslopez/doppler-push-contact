exports = async function () {
  const simulateProcess = true; // when simulate, the cleaning info is registered in the collection, but the push_contacts are NOT REMOVED.
  const clusterName = "Push";
  const dbName = "push-int"; // replace for the proper db name
  const daysSlot = 365;

  const filterDateTo = new Date();
  filterDateTo.setHours(0);
  filterDateTo.setMinutes(0);
  filterDateTo.setSeconds(0);
  filterDateTo.setMilliseconds(0);
  filterDateTo.setDate(filterDateTo.getDate() - daysSlot);

  // WARNING: use to run with harcoded dates
  //const filterDateTo = new Date('2023-01-01T00:00:00.000Z');

  // domains filter can be used with testing purposes
  const domainsFilter = [
    //{ name: 'myDomain.toTest.com.ar' },
  ];
  // can define a number of domains to be processed
  const limitFilter = 0;

  // prepare collections
  const mongoCluster = context.services.get(clusterName);
  const pushContactCollection = mongoCluster
    .db(dbName)
    .collection("push-contacts");
  const domainCollection = mongoCluster.db(dbName).collection("domains");
  const domainDeletedContactSumarizationCollection = mongoCluster
    .db(dbName)
    .collection("domain_deleted_contact_summarization");

  // to register date of processing
  const processDate = new Date();

  // to calculate process time
  const processStart = new Date();

  const processDomain = async (domain) => {
    const domainStatistics = {
      contactsTotal: 0,
      contactsToRemove: 0,
      contactsRemoved: 0,
    };

    // count quantity of contacts for current domain
    domainStatistics.contactsTotal = await pushContactCollection.count({
      domain: domain.name,
    });

    await pushContactCollection
      .aggregate([
        {
          $match: {
            domain: domain.name,
            deleted: true,
          },
        },
        {
          $unwind: {
            path: "$history_events",
            preserveNullAndEmptyArrays: false,
          },
        },
        {
          $project: {
            domain: 1,
            device_token: 1,
            email: 1,
            deleted: 1,
            inserted_date: "$history_events.inserted_date",
          },
        },
        {
          $group: {
            _id: {
              device_token: "$device_token",
              _id: "$_id",
            },
            lastInsertedDate: { $max: "$inserted_date" },
          },
        },
        {
          $project: {
            device_token: "$_id.device_token",
            id: "$_id._id",
            lastInsertedDate: 1,
          },
        },
        {
          $match: {
            lastInsertedDate: { $lte: filterDateTo },
          },
        },
      ])
      .toArray()
      .then(async (contacts) => {
        // the aggregation return ONLY ONE item corresponding to history_events summarization for current message
        for (const contact of contacts) {
          domainStatistics.contactsToRemove++;
          console.log(
            `-- CONTACT to remove: ${contact.id}, last inserted date: ${contact.lastInsertedDate.toUTCString()}`,
          );

          // DELETE CONTACT
          const removeResult =
            !simulateProcess &&
            (await pushContactCollection.deleteOne({ _id: contact.id }));

          if (
            simulateProcess ||
            (removeResult && removeResult.deletedCount == 1)
          ) {
            domainStatistics.contactsRemoved++;
          } else {
            console.log(`FAIL DELETE for id: ${contact.id}`);
          }
        }
        return contacts;
      })
      .catch((err) => console.error(`Failed to remove push contacts: ${err}`));

    return domainStatistics;
  };

  if (simulateProcess) {
    console.log("SIMULATING...");
  } else {
    console.log("IT IS NOT A SIMULATION!!");
  }
  console.log(
    `Processing deleted Contacts who have their last history_event registered before: ${filterDateTo.toUTCString()} (by domain).`,
  );

  const domainsPipeline = [];
  if (domainsFilter.length > 0) {
    domainsPipeline.push({
      $match: {
        $or: domainsFilter,
      },
    });
  }

  domainsPipeline.push({ $sort: { _id: 1 } });

  if (limitFilter > 0) {
    domainsPipeline.push({ $limit: limitFilter });
  }

  let domainNumber = 0;
  let totalContactsRemovedForAllDomains = 0;
  await domainCollection
    .aggregate(domainsPipeline)
    .toArray()
    .then(async (domains) => {
      for (const domain of domains) {
        domainNumber++;
        console.log(
          `-------------------------- DOMAIN ${domainNumber}: ${domain.name}, _id: ${domain._id}`,
        );
        try {
          const dStatistics = await processDomain(domain);

          totalContactsRemovedForAllDomains =
            totalContactsRemovedForAllDomains + dStatistics.contactsRemoved;
          if (dStatistics.contactsToRemove > 0) {
            console.log("(NEED TO REMOVE CONTACTS FOR THIS DOMAIN)");
          }

          domainDeletedContactSumarizationCollection.insertOne({
            domain: domain.name,
            history_event_registered_before_to: filterDateTo,
            contact_total: dStatistics.contactsTotal,
            contact_to_be_deleted: dStatistics.contactsToRemove,
            contact_deleted: dStatistics.contactsRemoved,
            processed_date: processDate,
          });

          console.log(
            `*** RESULT ${domainNumber} (${domain.name}): total contacts: ${dStatistics.contactsTotal}, contacts to remove: ${dStatistics.contactsToRemove}, contacts removed: ${dStatistics.contactsRemoved}`,
          );
        } catch (err) {
          console.error(`Failed processing domain ${domain.name}: ${err}`);
        }
      }
      return domains;
    })
    .catch((err) => console.error(`Failed to find domains: ${err}`));

  const processEnd = new Date();
  const processTime = processEnd.getTime() - processStart.getTime();
  const processTimeSeconds = processTime / 1000;

  console.log(
    `Was removed ${totalContactsRemovedForAllDomains} contacts for ${domainNumber} domains in ${processTimeSeconds} seconds...`,
  );
};
