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
                { "_id", key },
                { "domain", pushContactModel.Domain },
                { "device_token", pushContactModel.DeviceToken },
                { "email", pushContactModel.Email },
                { "deleted", false },
                { "modified", now }
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

            var filter = FilterBuilder.Eq("domain", pushContactFilter.Domain) & !FilterBuilder.Eq("deleted", true);

            try
            {
                var pushContactsFiltered = await (await PushContacts.FindAsync<BsonDocument>(filter)).ToListAsync();

                return pushContactsFiltered
                    .Select(x =>
                    {
                        return new PushContactModel
                        {
                            Domain = x["domain"].AsString,
                            DeviceToken = x["device_token"].AsString,
                            Email = x["email"].AsString
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

            var filter = Builders<BsonDocument>.Filter.AnyIn("device_token", deviceTokens);

            var update = new BsonDocument("$set", new BsonDocument
                {
                    { "deleted", true },
                    { "modified", DateTime.UtcNow }
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
