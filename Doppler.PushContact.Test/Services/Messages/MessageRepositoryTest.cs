using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using Microsoft.Extensions.Logging;
using AutoFixture;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using Xunit;

namespace Doppler.PushContact.Test.Services.Messages
{
    public class MessageRepositoryTest
    {
        private static MessageRepository CreateSut(
        IMongoClient mongoClient = null,
        IOptions<PushMongoContextSettings> pushMongoContextSettings = null,
        ILogger<MessageRepository> logger = null)
        {
            return new MessageRepository(
                mongoClient ?? Mock.Of<IMongoClient>(),
                pushMongoContextSettings ?? Mock.Of<IOptions<PushMongoContextSettings>>(),
                logger ?? Mock.Of<ILogger<MessageRepository>>());
        }

        [Fact]
        public async Task GetMessageDomainAsync_should_return_null_when_message_does_not_exist()
        {
            // Arrange
            var fixture = new Fixture();
            var messageId = fixture.Create<Guid>();

            var mongoClientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var collectionMock = new Mock<IMongoCollection<BsonDocument>>();
            var asyncCursorMock = new Mock<IAsyncCursor<BsonDocument>>();

            var settings = Options.Create(new PushMongoContextSettings
            {
                DatabaseName = "TestDatabase",
                MessagesCollectionName = "TestCollection"
            });

            // Configure the cursor to return no elements
            asyncCursorMock
                .SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            asyncCursorMock
                .SetupGet(x => x.Current)
                .Returns(new List<BsonDocument>());

            collectionMock
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(asyncCursorMock.Object);

            databaseMock
                .Setup(x => x.GetCollection<BsonDocument>(settings.Value.MessagesCollectionName, null))
                .Returns(collectionMock.Object);

            mongoClientMock
                .Setup(x => x.GetDatabase(settings.Value.DatabaseName, null))
                .Returns(databaseMock.Object);

            var sut = CreateSut(
                mongoClientMock.Object,
                settings);

            // Act
            var result = await sut.GetMessageDomainAsync(messageId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMessageDomainAsync_should_return_domain_when_message_exists()
        {
            // Arrange
            var fixture = new Fixture();
            var messageId = fixture.Create<Guid>();

            var expectedDomain = "example.com";
            var document = new BsonDocument
            {
                { MessageDocumentProps.DomainPropName, expectedDomain }
            };

            var mongoClientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var collectionMock = new Mock<IMongoCollection<BsonDocument>>();
            var asyncCursorMock = new Mock<IAsyncCursor<BsonDocument>>();

            var settings = Options.Create(new PushMongoContextSettings
            {
                DatabaseName = "TestDatabase",
                MessagesCollectionName = "TestCollection"
            });

            // Configure the cursor to return the expected document
            asyncCursorMock
                .SetupSequence(x => x.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            asyncCursorMock
                .SetupGet(x => x.Current)
                .Returns(new List<BsonDocument> { document });

            collectionMock
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .ReturnsAsync(asyncCursorMock.Object);

            databaseMock
                .Setup(x => x.GetCollection<BsonDocument>(settings.Value.MessagesCollectionName, null))
                .Returns(collectionMock.Object);

            mongoClientMock
                .Setup(x => x.GetDatabase(settings.Value.DatabaseName, null))
                .Returns(databaseMock.Object);

            var sut = CreateSut(
                mongoClientMock.Object,
                settings);

            // Act
            var result = await sut.GetMessageDomainAsync(messageId);

            // Assert
            Assert.Equal(expectedDomain, result);
        }

        [Fact]
        public async Task GetMessageDomainAsync_should_log_error_when_mongo_exception_is_thrown()
        {
            // Arrange
            var fixture = new Fixture();
            var messageId = fixture.Create<Guid>();

            var mongoClientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var collectionMock = new Mock<IMongoCollection<BsonDocument>>();
            var loggerMock = new Mock<ILogger<MessageRepository>>();

            var settings = Options.Create(new PushMongoContextSettings
            {
                DatabaseName = "TestDatabase",
                MessagesCollectionName = "TestCollection"
            });

            collectionMock
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .Throws(new MongoException("Test exception"));

            databaseMock
                .Setup(x => x.GetCollection<BsonDocument>(settings.Value.MessagesCollectionName, null))
                .Returns(collectionMock.Object);

            mongoClientMock
                .Setup(x => x.GetDatabase(settings.Value.DatabaseName, null))
                .Returns(databaseMock.Object);

            var sut = CreateSut(
                mongoClientMock.Object,
                settings,
                logger: loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<MongoException>(() => sut.GetMessageDomainAsync(messageId));

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"MongoException getting Message by {nameof(messageId)} {messageId}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetMessageDomainAsync_should_log_error_when_general_exception_is_thrown()
        {
            // Arrange
            var fixture = new Fixture();
            var messageId = fixture.Create<Guid>();

            var mongoClientMock = new Mock<IMongoClient>();
            var databaseMock = new Mock<IMongoDatabase>();
            var collectionMock = new Mock<IMongoCollection<BsonDocument>>();
            var loggerMock = new Mock<ILogger<MessageRepository>>();

            var settings = Options.Create(new PushMongoContextSettings
            {
                DatabaseName = "TestDatabase",
                MessagesCollectionName = "TestCollection"
            });

            collectionMock
                .Setup(x => x.FindAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<FindOptions<BsonDocument, BsonDocument>>(), default))
                .Throws(new Exception("Test exception"));

            databaseMock
                .Setup(x => x.GetCollection<BsonDocument>(settings.Value.MessagesCollectionName, null))
                .Returns(collectionMock.Object);

            mongoClientMock
                .Setup(x => x.GetDatabase(settings.Value.DatabaseName, null))
                .Returns(databaseMock.Object);

            var sut = CreateSut(
                mongoClientMock.Object,
                settings,
                logger: loggerMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => sut.GetMessageDomainAsync(messageId));

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Unexpected error getting Message by {nameof(messageId)} {messageId}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
