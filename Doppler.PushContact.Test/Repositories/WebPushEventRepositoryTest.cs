using Doppler.PushContact.Repositories;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.Test.Repositories
{
    public class WebPushEventRepositoryTest
    {
        private readonly Mock<IMongoClient> _mockMongoClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<BsonDocument>> _mockCollection;
        private readonly Mock<IOptions<PushMongoContextSettings>> _mockSettings;
        private readonly Mock<ILogger<WebPushEventRepository>> _mockLogger;
        private readonly WebPushEventRepository _repository;

        public WebPushEventRepositoryTest()
        {
            _mockMongoClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<BsonDocument>>();
            _mockSettings = new Mock<IOptions<PushMongoContextSettings>>();
            _mockLogger = new Mock<ILogger<WebPushEventRepository>>();

            _mockSettings.Setup(s => s.Value).Returns(new PushMongoContextSettings
            {
                DatabaseName = "testdb",
                WebPushEventCollectionName = "webPushEvents"
            });

            _mockMongoClient.Setup(c => c.GetDatabase(It.IsAny<string>(), null))
                .Returns(_mockDatabase.Object);

            _mockDatabase.Setup(d => d.GetCollection<BsonDocument>(It.IsAny<string>(), null))
                .Returns(_mockCollection.Object);

            _repository = new WebPushEventRepository(_mockMongoClient.Object, _mockSettings.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetWebPushEventSummarization_ShouldReturnSummarizedData_WhenDataExists()
        {
            // Arrange
            var messageId = Guid.NewGuid();
            var formattedMessageId = new BsonBinaryData(messageId, GuidRepresentation.Standard);

            var documents = new List<BsonDocument>
            {
                new BsonDocument
                {
                    { "_id", formattedMessageId },
                    { "Delivered", 1 },
                    { "NotDelivered", 1 }
                }
            };

            var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
            mockCursor.Setup(_ => _.Current).Returns(documents);
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<BsonDocument, BsonDocument>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetWebPushEventSummarization(messageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(messageId, result.MessageId);
            Assert.Equal(2, result.SentQuantity);
            Assert.Equal(1, result.Delivered);
            Assert.Equal(1, result.NotDelivered);
        }

        [Fact]
        public async Task GetWebPushEventSummarization_ShouldReturnEmptyStats_WhenNoDataExists()
        {
            // Arrange
            var messageId = Guid.NewGuid();

            var mockCursor = new Mock<IAsyncCursor<BsonDocument>>();
            mockCursor.Setup(_ => _.Current).Returns(new List<BsonDocument>());
            mockCursor
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(c => c.AggregateAsync(
                It.IsAny<PipelineDefinition<BsonDocument, BsonDocument>>(),
                It.IsAny<AggregateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _repository.GetWebPushEventSummarization(messageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(messageId, result.MessageId);
            Assert.Equal(0, result.SentQuantity);
            Assert.Equal(0, result.Delivered);
            Assert.Equal(0, result.NotDelivered);
        }
    }
}
