var simulateProcess = true; // when simulate, the cleaning info is registered in the collection, but the push_contacts are NOT REMOVED
const daysSlot = 365;

const filterDateTo = new Date();
filterDateTo.setHours(0);
filterDateTo.setMinutes(0);
filterDateTo.setSeconds(0);
filterDateTo.setMilliseconds(0);
filterDateTo.setDate(filterDateTo.getDate() - daysSlot);

// WARNING: use to run with harcoded dates
//const filterDateTo = new Date('2023-01-01T00:00:00.000Z');

// to register date of processing
var processDate = new ISODate();

// to calculate process time
var processStart = new Date();

if (simulateProcess) {
  print("SIMULATING...");
} else {
  print("IT IS NOT A SIMULATION!!");
}

print(
  `Processing deleted Contacts who have their last history_event registered before: ${filterDateTo.toUTCString()} (by domain).`,
);

var cursorDomains = db.domains
  .find({
    // WARNING: use to run with harcoded domains
    //$or: [
    //	{ name: 'myDomain.toTest.com.ar' },
    //],
    //_id: { $gte: '6294e431f9534184f9dccc9d' }
  })
  .sort({ _id: 1 })
  .limit(0);

var totalContactsRemovedForAllDomains = 0;
var domainNumber = 0;

while (cursorDomains.hasNext()) {
  var domain = cursorDomains.next();
  domainNumber++;

  print(
    `-------------------------- DOMAIN ${domainNumber}: ${domain.name}, _id: ${domain._id}, modified: ${domain.modified.toUTCString()}`,
  );

  // count quantity of contacts for current domain
  var contactsByDomain = db
    .getCollection("push-contacts")
    .countDocuments({ domain: domain.name });

  var removeContactsList = db
    .getCollection("push-contacts")
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
          _id: "$_id._id",
          lastInsertedDate: 1,
        },
      },
      {
        $match: {
          lastInsertedDate: { $lte: filterDateTo },
        },
      },
    ])
    .map(function (contact) {
      return {
        id: contact._id,
        deviceToken: contact.device_token,
        lastInsertedDate: contact.lastInsertedDate,
      };
    });

  var removedContacts = 0;
  var contactsToRemove = 0;
  print("------ Remove contacts");

  removeContactsList.forEach((contact) => {
    contactsToRemove++;
    print(
      `-- CONTACT to remove: ${contact.id}, last inserted date: ${contact.lastInsertedDate.toUTCString()}, deviceToken: ${contact.deviceToken}`,
    );

    var removeResult =
      !simulateProcess &&
      db.getCollection("push-contacts").deleteOne({ _id: contact.id });

    if (simulateProcess || (removeResult && removeResult.deletedCount == 1)) {
      removedContacts++;
      totalContactsRemovedForAllDomains++;
    } else {
      print("No pudo borrarlo");
    }
  });

  print(
    `*** RESULT ${domainNumber}: ${domain.name}, total contacts by domain: ${contactsByDomain}, contacts to remove: ${contactsToRemove}, contacts removed: ${removedContacts}`,
  );

  if (contactsToRemove > 0) {
    print("(NEED TO REMOVE CONTACTS FOR THIS DOMAIN)");
  }

  db.getCollection("domain_deleted_contact_summarization").insertOne({
    domain: domain.name,
    history_event_registered_before_to: filterDateTo,
    contact_total: contactsByDomain,
    contact_to_be_deleted: contactsToRemove,
    contact_deleted: removedContacts,
    processed_date: processDate,
  });
}

var processEnd = new Date();
var processTime = processEnd.getTime() - processStart.getTime();
var processTimeSeconds = processTime / 1000;

print(
  `${totalContactsRemovedForAllDomains} contacts were removed for ${domainNumber} domains in ${processTimeSeconds}...`,
);
