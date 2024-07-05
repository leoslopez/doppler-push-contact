using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Repositories.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories
{
    public class WebPushEventRepository : IWebPushEventRepository
    {
        private readonly IMongoCollection<BsonDocument> _eventsCollection;

        public WebPushEventRepository(IMongoDatabase database)
        {
            // TODO: move collection name into config file
            _eventsCollection = database.GetCollection<BsonDocument>("webPushEvent");
        }

        public async Task<bool> InsertAsync(WebPushEvent webPushEvent, CancellationToken cancellationToken)
        {
            var eventBsonDocument = webPushEvent.ToBsonDocument();

            await _eventsCollection.InsertOneAsync(
                document: eventBsonDocument,
                options: default,
                cancellationToken: cancellationToken
            );

            return true;
        }
    }
}
