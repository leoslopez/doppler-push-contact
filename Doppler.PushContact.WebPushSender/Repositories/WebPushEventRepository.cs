using Doppler.PushContact.Models.Entities;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Repositories.Setup;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories
{
    public class WebPushEventRepository : IWebPushEventRepository
    {
        private readonly IMongoCollection<BsonDocument> _eventsCollection;

        public WebPushEventRepository(IMongoDatabase database, IOptions<RepositorySettings> repositorySettings)
        {
            _eventsCollection = database.GetCollection<BsonDocument>(repositorySettings.Value.WebPushEventCollectionName);
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
