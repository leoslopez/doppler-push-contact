using Doppler.PushContact.DTOs;
using Doppler.PushContact.Repositories.Interfaces;
using Doppler.PushContact.Services;
using Doppler.PushContact.Services.Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Repositories
{
    public class WebPushEventRepository : IWebPushEventRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOptions<PushMongoContextSettings> _pushMongoContextSettings;
        private readonly ILogger<MessageRepository> _logger;

        public WebPushEventRepository(
            IMongoClient mongoClient,
            IOptions<PushMongoContextSettings> pushMongoContextSettings,
            ILogger<MessageRepository> logger)
        {

            _mongoClient = mongoClient;
            _pushMongoContextSettings = pushMongoContextSettings;
            _logger = logger;
        }

        public async Task<WebPushEventSummarizationDTO> GetWebPushEventSummarization(Guid messageId)
        {
            var formattedMessageId = new BsonBinaryData(messageId, GuidRepresentation.Standard);

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq(WebPushEventDocumentProps.MessageId_PropName, formattedMessageId)
            );

            var aggregation = WebPushEvents.Aggregate()
                .Match(filter)
                .Group(new BsonDocument
                {
                    { "_id", "$" + WebPushEventDocumentProps.MessageId_PropName },
                    { "SentQuantity", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                        {
                            new BsonDocument( "$in", new BsonArray {
                                "$" + WebPushEventDocumentProps.Type_PropName, new BsonArray {
                                    (int)WebPushEventType.Delivered,
                                    (int)WebPushEventType.DeliveryFailed,

                                }
                            }),
                            1,
                            0
                        })
                    )},
                    { "Delivered", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray {
                                "$" + WebPushEventDocumentProps.Type_PropName, (int)WebPushEventType.Delivered }),
                            1,
                            0
                        })
                    )}
                });

            var result = await aggregation.FirstOrDefaultAsync();

            if (result == null)
            {
                return new WebPushEventSummarizationDTO
                {
                    MessageId = messageId,
                    SentQuantity = 0,
                    Delivered = 0,
                    NotDelivered = 0,
                };
            }

            return new WebPushEventSummarizationDTO
            {
                MessageId = messageId,
                SentQuantity = result["SentQuantity"].AsInt32,
                Delivered = result["Delivered"].AsInt32,
                NotDelivered = result["SentQuantity"].AsInt32 - result["Delivered"].AsInt32,
            };
        }

        private IMongoCollection<BsonDocument> WebPushEvents
        {
            get
            {
                var database = _mongoClient.GetDatabase(_pushMongoContextSettings.Value.DatabaseName);
                return database.GetCollection<BsonDocument>(_pushMongoContextSettings.Value.WebPushEventCollectionName);
            }
        }
    }
}
