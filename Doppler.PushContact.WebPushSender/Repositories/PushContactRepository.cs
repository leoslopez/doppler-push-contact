using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Repositories.Setup;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories
{
    public class PushContactRepository : IPushContactRepository
    {
        private readonly IMongoCollection<BsonDocument> _collection;

        public PushContactRepository(IMongoDatabase database, IOptions<RepositorySettings> repositorySettings)
        {
            _collection = database.GetCollection<BsonDocument>(repositorySettings.Value.PushContactsCollectionName);
        }

        public async Task<bool> MarkDeletedAsync(string pushContactId)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", pushContactId);

            var update = Builders<BsonDocument>.Update.Set("deleted", true);

            var result = await _collection.UpdateOneAsync(filter, update);

            return result != null && result.ModifiedCount > 0;
        }

    }
}
