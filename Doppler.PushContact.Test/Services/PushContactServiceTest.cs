using AutoFixture;
using Doppler.PushContact.Models;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
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
    }
}
