using Doppler.PushContact.DTOs;
using Doppler.PushContact.Models.Entities;
using Doppler.PushContact.Models.Enums;
using Doppler.PushContact.Repositories.Interfaces;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.Repositories
{
    public class WebPushEventRepository : IWebPushEventRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOptions<PushMongoContextSettings> _pushMongoContextSettings;
        private readonly ILogger<WebPushEventRepository> _logger;

        public WebPushEventRepository(
            IMongoClient mongoClient,
            IOptions<PushMongoContextSettings> pushMongoContextSettings,
            ILogger<WebPushEventRepository> logger)
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
                    // TODO: consider re-analyze summarization when ProcessingFailed and DeliveryFailedButRetry will be treated
                    { "NotDelivered", new BsonDocument("$sum", new BsonDocument("$cond", new BsonArray
                        {
                            new BsonDocument("$in", new BsonArray {
                                "$" + WebPushEventDocumentProps.Type_PropName, new BsonArray {
                                    (int)WebPushEventType.DeliveryFailed,
                                    (int)WebPushEventType.ProcessingFailed,
                                    (int)WebPushEventType.DeliveryFailedButRetry
                                }}),
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
                SentQuantity = result["NotDelivered"].AsInt32 + result["Delivered"].AsInt32,
                Delivered = result["Delivered"].AsInt32,
                NotDelivered = result["NotDelivered"].AsInt32,
            };
        }

        public async Task<bool> InsertAsync(WebPushEvent webPushEvent, CancellationToken cancellationToken)
        {
            var eventBsonDocument = webPushEvent.ToBsonDocument();

            await WebPushEvents.InsertOneAsync(
                document: eventBsonDocument,
                options: default,
                cancellationToken: cancellationToken
            );

            return true;
        }

        public async Task<bool> IsWebPushEventRegistered(string pushContactId, Guid messageId, WebPushEventType type)
        {
            var formattedMessageId = new BsonBinaryData(messageId, GuidRepresentation.Standard);

            var filter = Builders<BsonDocument>.Filter.And(
                Builders<BsonDocument>.Filter.Eq(WebPushEventDocumentProps.PushContactId_PropName, pushContactId),
                Builders<BsonDocument>.Filter.Eq(WebPushEventDocumentProps.Type_PropName, (int)type),
                Builders<BsonDocument>.Filter.Eq(WebPushEventDocumentProps.MessageId_PropName, formattedMessageId)
            );

            try
            {
                var webPushEvent = await WebPushEvents.Find(filter).FirstOrDefaultAsync();
                return webPushEvent != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking if WebPushEvent exists for pushContactId: {pushContactId}, messageId: {messageId}, type: {type}",
                    pushContactId,
                    messageId,
                    type.ToString()
                );
                throw;
            }
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
