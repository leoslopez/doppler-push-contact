using Doppler.PushContact.Models.Entities;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Repositories.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories
{
    public class PushContactRepository : IPushContactRepository
    {
        private readonly IMongoCollection<BsonDocument> _collection;
        private readonly ILogger<PushContactRepository> _logger;

        public PushContactRepository(
            IMongoDatabase database,
            IOptions<RepositorySettings> repositorySettings,
            ILogger<PushContactRepository> logger
        )
        {
            _collection = database.GetCollection<BsonDocument>(repositorySettings.Value.PushContactsCollectionName);
            _logger = logger;
        }

        public async Task<bool> MarkDeletedAsync(string pushContactId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq(PushContactDocumentProps.IdPropName, pushContactId);

            var update = Builders<BsonDocument>.Update
                .Set(PushContactDocumentProps.DeletedPropName, true)
                .Set(PushContactDocumentProps.ModifiedPropName, DateTime.UtcNow);

            try
            {
                var result = await _collection.UpdateOneAsync(filter, update);
                return result != null && result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error marking PushContact deleted (_id: {pushContactId})");
                return false;
            }
        }
    }
}
