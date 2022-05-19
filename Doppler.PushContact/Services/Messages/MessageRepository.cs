using Doppler.PushContact.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace Doppler.PushContact.Services.Messages
{
    public class MessageRepository : IMessageRepository
    {
        private readonly IMongoClient _mongoClient;
        private readonly IOptions<PushMongoContextSettings> _pushMongoContextSettings;
        private readonly ILogger<MessageRepository> _logger;

        public MessageRepository(
            IMongoClient mongoClient,
            IOptions<PushMongoContextSettings> pushMongoContextSettings,
            ILogger<MessageRepository> logger)
        {

            _mongoClient = mongoClient;
            _pushMongoContextSettings = pushMongoContextSettings;
            _logger = logger;
        }

        public async Task AddAsync(Guid messageId, string domain, string title, string body, string onClickLink, int sent, int delivered, int notDelivered)
        {
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException($"'{nameof(domain)}' cannot be null or empty.", nameof(domain));
            }

            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentException($"'{nameof(body)}' cannot be null or empty.", nameof(body));
            }

            var now = DateTime.UtcNow;
            var key = ObjectId.GenerateNewId(now).ToString();

            var messageDocument = new BsonDocument {
                { MessageDocumentProps.IdPropName, key },
                { MessageDocumentProps.MessageIdPropName, new BsonBinaryData(messageId, GuidRepresentation.Standard) },
                { MessageDocumentProps.DomainPropName, domain },
                { MessageDocumentProps.TitlePropName, title },
                { MessageDocumentProps.BodyPropName, body },
                { MessageDocumentProps.OnClickLinkPropName, string.IsNullOrEmpty(onClickLink) ? BsonNull.Value : onClickLink },
                { MessageDocumentProps.SentPropName, sent },
                { MessageDocumentProps.DeliveredPropName, delivered },
                { MessageDocumentProps.NotDeliveredPropName, notDelivered },
                { MessageDocumentProps.InsertedDatePropName, now }
            };

            try
            {
                await Messages.InsertOneAsync(messageDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, @$"Error inserting message with {nameof(messageId)} {messageId}");

                throw;
            }
        }

        public async Task<MessageDetails> GetMessageDetailsAsync(string domain, Guid messageId)
        {
            var filterBuilder = Builders<BsonDocument>.Filter;

            var filter = filterBuilder.Eq(MessageDocumentProps.DomainPropName, domain);

            filter &= filterBuilder.Eq(MessageDocumentProps.MessageIdPropName, new BsonBinaryData(messageId, GuidRepresentation.Standard));

            try
            {
                BsonDocument message = await (await Messages.FindAsync<BsonDocument>(filter)).SingleOrDefaultAsync();

                return new MessageDetails
                {
                    MessageId = message.GetValue(MessageDocumentProps.MessageIdPropName).AsGuid,
                    Domain = message.GetValue(MessageDocumentProps.DomainPropName).AsString,
                    Title = message.GetValue(MessageDocumentProps.TitlePropName).AsString,
                    Body = message.GetValue(MessageDocumentProps.BodyPropName).AsString,
                    OnClickLinkPropName = message.GetValue(MessageDocumentProps.OnClickLinkPropName) == BsonNull.Value ? null : message.GetValue(MessageDocumentProps.OnClickLinkPropName).AsString,
                    Sent = message.GetValue(MessageDocumentProps.SentPropName).AsInt32,
                    Delivered = message.GetValue(MessageDocumentProps.DeliveredPropName).AsInt32,
                    NotDelivered = message.GetValue(MessageDocumentProps.NotDeliveredPropName).AsInt32
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting message with {nameof(domain)} {domain} and {nameof(messageId)} {messageId}");

                throw;
            }
        }

        private IMongoCollection<BsonDocument> Messages
        {
            get
            {
                var database = _mongoClient.GetDatabase(_pushMongoContextSettings.Value.DatabaseName);
                return database.GetCollection<BsonDocument>(_pushMongoContextSettings.Value.MessagesCollectionName);
            }
        }
    }
}
