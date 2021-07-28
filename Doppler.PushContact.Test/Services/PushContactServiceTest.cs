using AutoFixture;
using Doppler.PushContact.Models;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.Test.Services
{
    public class PushContactServiceTest
    {
        private static PushContactService CreateSut(
            IMongoClient mongoClient = null,
            IOptions<PushContactMongoContextSettings> pushContactMongoContextSettings = null,
            ILogger<PushContactService> logger = null)
        {
            return new PushContactService(
                mongoClient ?? Mock.Of<IMongoClient>(),
                pushContactMongoContextSettings ?? Mock.Of<IOptions<PushContactMongoContextSettings>>(),
                logger ?? Mock.Of<ILogger<PushContactService>>());
        }

        [Fact]
        public async Task AddAsync_should_throw_argument_null_exception_when_push_contact_model_is_null()
        {
            // Arrange
            PushContactModel pushContactModel = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.AddAsync(pushContactModel));
        }

        [Fact]
        public async Task AddAsync_should_return_false_and_log_error_when_a_push_contact_model_cannot_be_added()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactModel = fixture.Create<PushContactModel>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.MongoPushContactCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.MongoPushContactDatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            var result = await sut.AddAsync(pushContactModel);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == @$"Error inserting {nameof(pushContactModel)}
with following {nameof(pushContactModel.DeviceToken)}: {pushContactModel.DeviceToken}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task AddAsync_should_return_true_when_a_push_contact_model_can_be_added()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactModel = fixture.Create<PushContactModel>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollection = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollection
                .Setup(x => x.InsertOneAsync(It.IsAny<BsonDocument>(), null, default))
                .Returns(Task.CompletedTask);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.MongoPushContactCollectionName, null))
                .Returns(pushContactsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.MongoPushContactDatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            var result = await sut.AddAsync(pushContactModel);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetAsync_should_throw_argument_null_exception_when_push_contact_filter_is_null()
        {
            // Arrange
            PushContactFilter pushContactFilter = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.GetAsync(pushContactFilter));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GetAsync_should_throw_argument_exception_when_push_contact_filter_domain_is_null_or_empty
            (string domain)
        {
            // Arrange
            var pushContactFilter = new PushContactFilter(domain);

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.GetAsync(pushContactFilter));
        }

        [Fact]
        public async Task GetAsync_should_return_a_empty_collection_and_log_error_when_push_contacts_cannot_be_getter()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactFilter = fixture.Create<PushContactFilter>();

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.MongoPushContactCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.MongoPushContactDatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            Assert.Empty(result);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Error getting {nameof(PushContactModel)}s"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetAsync_should_return_push_contacts_filtered_by_domain()
        {
            // Arrange
            List<BsonDocument> allPushContactDocuments = FakePushContactDocuments(10);

            var random = new Random();
            int randomPushContactIndex = random.Next(allPushContactDocuments.Count);
            var pushContactFilter = new PushContactFilter(allPushContactDocuments[randomPushContactIndex]["domain"].AsString);

            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            pushContactsCursorMock
                .Setup(_ => _.Current)
                .Returns(allPushContactDocuments.Where(x => x["domain"].AsString == pushContactFilter.Domain));
            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true);

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(pushContactsCursorMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.MongoPushContactCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.MongoPushContactDatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            Assert.True(result.All(x => x.Domain == pushContactFilter.Domain));
        }

        private static List<BsonDocument> FakePushContactDocuments(int count)
        {
            var fixture = new Fixture();

            var pushContactDocumentKeys = new[] { "_id", "domain", "device_token", "modified" };
            Generator<string> pushContactDocumentValues = fixture.Create<Generator<string>>();

            return Enumerable.Repeat(0, count)
                .Select(s => pushContactDocumentKeys.Zip(pushContactDocumentValues, Tuple.Create)
                .ToDictionary(t => t.Item1, t => t.Item2))
                .Select(x => { return new BsonDocument(x); })
                .ToList();
        }
    }
}
