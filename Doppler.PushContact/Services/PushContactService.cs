using Doppler.PushContact.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Doppler.PushContact.Services
{
    public class PushContactService : IPushContactService
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOptions<PushContactMongoContextSettings> _pushContactMongoContextSettings;
        private readonly ILogger<PushContactService> _logger;

        private const string PushContactDocumentIdPropName = "_id";
        private const string PushContactDocumentDomainPropName = "domain";
        private const string PushContactDocumentDeviceTokenPropName = "device_token";
        private const string PushContactDocumentEmailPropName = "email";
        private const string PushContactDocumentDeletedPropName = "deleted";
        private const string PushContactDocumentModifiedPropName = "modified";

        private const string PushContactDocumentHistoryEventsPropName = "history_events";
        private const string PushContactDocumentHistoryEvents_SentSuccessPropName = "sent_success";
        private const string PushContactDocumentHistoryEvents_EventDatePropName = "event_date";
        private const string PushContactDocumentHistoryEvents_InsertedDatePropName = "inserted_date";
        private const string PushContactDocumentHistoryEvents_DetailsPropName = "details";

        public PushContactService(
            IMongoClient mongoClient,
            IOptions<PushContactMongoContextSettings> pushContactMongoContextSettings,
            ILogger<PushContactService> logger)
        {

            _mongoClient = mongoClient;
            _pushContactMongoContextSettings = pushContactMongoContextSettings;
            _logger = logger;
        }

        public async Task<bool> AddAsync(PushContactModel pushContactModel)
        {
            if (pushContactModel == null)
            {
                throw new ArgumentNullException(nameof(pushContactModel));
            }

            var now = DateTime.UtcNow;
            var key = ObjectId.GenerateNewId(now).ToString();

            var pushContactDocument = new BsonDocument {
                { PushContactDocumentIdPropName, key },
                { PushContactDocumentDomainPropName, pushContactModel.Domain },
                { PushContactDocumentDeviceTokenPropName, pushContactModel.DeviceToken },
                { PushContactDocumentEmailPropName, pushContactModel.Email },
                { PushContactDocumentDeletedPropName, false },
                { PushContactDocumentModifiedPropName, now }
            };

            try
            {
                await PushContacts.InsertOneAsync(pushContactDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error inserting {nameof(pushContactModel)}
with following {nameof(pushContactModel.DeviceToken)}: {pushContactModel.DeviceToken}");

                return false;
            }

            return true;
        }

        public async Task UpdateEmailAsync(string deviceToken, string email)
        {
            if (deviceToken == null)
            {
                throw new ArgumentNullException(nameof(deviceToken));
            }

            if (email == null)
            {
                throw new ArgumentNullException(nameof(email));
            }

            var filter = Builders<BsonDocument>.Filter.Eq(PushContactDocumentDeviceTokenPropName, deviceToken);

            var updateDefinition = Builders<BsonDocument>.Update
                .Set(PushContactDocumentEmailPropName, email)
                .Set(PushContactDocumentModifiedPropName, DateTime.UtcNow);

            try
            {
                await PushContacts.UpdateOneAsync(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error updating {nameof(PushContactModel)}
with {nameof(deviceToken)} {deviceToken}. {PushContactDocumentEmailPropName} can not be updated with following value: {email}");

                throw;
            }
        }

        public async Task<IEnumerable<PushContactModel>> GetAsync(PushContactFilter pushContactFilter)
        {
            if (pushContactFilter == null)
            {
                throw new ArgumentNullException(nameof(pushContactFilter));
            }

            if (string.IsNullOrEmpty(pushContactFilter.Domain))
            {
                throw new ArgumentException(
                    $"'{nameof(pushContactFilter.Domain)}' cannot be null or empty", nameof(pushContactFilter.Domain));
            }

            var FilterBuilder = Builders<BsonDocument>.Filter;

            var filter = FilterBuilder.Eq(PushContactDocumentDomainPropName, pushContactFilter.Domain) & !FilterBuilder.Eq(PushContactDocumentDeletedPropName, true);

            try
            {
                var pushContactsFiltered = await (await PushContacts.FindAsync<BsonDocument>(filter)).ToListAsync();

                return pushContactsFiltered
                    .Select(x =>
                    {
                        return new PushContactModel
                        {
                            Domain = x.GetValue(PushContactDocumentDomainPropName, null)?.AsString,
                            DeviceToken = x.GetValue(PushContactDocumentDeviceTokenPropName, null)?.AsString,
                            Email = x.GetValue(PushContactDocumentEmailPropName, null)?.AsString
                        };
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting {nameof(PushContactModel)}s");

                return Enumerable.Empty<PushContactModel>();
            }
        }

        public async Task<long> DeleteByDeviceTokenAsync(IEnumerable<string> deviceTokens)
        {
            if (deviceTokens == null || !deviceTokens.Any())
            {
                throw new ArgumentException(
                    $"'{nameof(deviceTokens)}' cannot be null or empty", nameof(deviceTokens));
            }

            var filter = Builders<BsonDocument>.Filter.AnyIn(PushContactDocumentDeviceTokenPropName, deviceTokens);

            var update = new BsonDocument("$set", new BsonDocument
                {
                    { PushContactDocumentDeletedPropName, true },
                    { PushContactDocumentModifiedPropName, DateTime.UtcNow }
                });

            try
            {
                var result = await PushContacts.UpdateManyAsync(filter, update);

                return result.IsModifiedCountAvailable ? result.ModifiedCount : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting {nameof(PushContactModel)}s");

                throw;
            }
        }

        public async Task AddHistoryEventsAsync(IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents)
        {
            if (pushContactHistoryEvents == null || !pushContactHistoryEvents.Any())
            {
                throw new ArgumentException(
                    $"'{nameof(pushContactHistoryEvents)}' cannot be null or empty", nameof(pushContactHistoryEvents));
            }

            var now = DateTime.UtcNow;

            var updateRequest = pushContactHistoryEvents
                .Select(x =>
                {
                    var historyEvent = new BsonDocument {
                        { PushContactDocumentHistoryEvents_SentSuccessPropName, x.SentSuccess },
                        { PushContactDocumentHistoryEvents_EventDatePropName, x.EventDate },
                        { PushContactDocumentHistoryEvents_InsertedDatePropName, now },
                        { PushContactDocumentHistoryEvents_DetailsPropName, x.Details }
                    };

                    var filter = Builders<BsonDocument>.Filter.Eq(PushContactDocumentDeviceTokenPropName, x.DeviceToken);

                    var update = Builders<BsonDocument>.Update.Push(PushContactDocumentHistoryEventsPropName, historyEvent);

                    return new UpdateOneModel<BsonDocument>(filter, update);
                });

            try
            {
                await PushContacts.BulkWriteAsync(updateRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding {nameof(PushContactHistoryEvent)}s");

                throw;
            }
        }

        private IMongoCollection<BsonDocument> PushContacts
        {
            get
            {
                var database = _mongoClient.GetDatabase(_pushContactMongoContextSettings.Value.DatabaseName);
                return database.GetCollection<BsonDocument>(_pushContactMongoContextSettings.Value.PushContactsCollectionName);
            }
        }
    }
}
