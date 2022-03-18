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
    public class DomainServiceTest
    {
        private static DomainService CreateSut(
            IMongoClient mongoClient = null,
            IOptions<PushMongoContextSettings> pushMongoContextSettings = null,
            ILogger<DomainService> logger = null)
        {
            return new DomainService(
                mongoClient ?? Mock.Of<IMongoClient>(),
                pushMongoContextSettings ?? Mock.Of<IOptions<PushMongoContextSettings>>(),
                logger ?? Mock.Of<ILogger<DomainService>>());
        }

        [Fact]
        public async Task UpsertAsync_should_throw_argument_null_exception_when_domain_is_null()
        {
            // Arrange
            Domain domain = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpsertAsync(domain));
        }

        [Fact]
        public async Task UpsertAsync_should_throw_exception_and_log_error_when_a_domain_cannot_be_upserted()
        {
            // Arrange
            var fixture = new Fixture();

            var domain = fixture.Create<Domain>();

            var pushMongoContextSettings = fixture.Create<PushMongoContextSettings>();

            var domainsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            domainsCollectionMock
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), It.IsAny<UpdateOptions>(), default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushMongoContextSettings.DomainsCollectionName, null))
                .Returns(domainsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<DomainService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.UpsertAsync(domain));

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Error upserting {nameof(Domain)} with {nameof(domain.Name)} {domain.Name}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UpsertAsync_should_not_throw_exception_when_a_domain_can_be_upserted()
        {
            // Arrange
            var fixture = new Fixture();

            var domain = fixture.Create<Domain>();

            var pushMongoContextSettings = fixture.Create<PushMongoContextSettings>();

            var updateResultMock = new Mock<UpdateResult>();
            var domainsCollection = new Mock<IMongoCollection<BsonDocument>>();
            domainsCollection
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), It.IsAny<UpdateOptions>(), default))
                .ReturnsAsync(updateResultMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushMongoContextSettings.DomainsCollectionName, null))
                .Returns(domainsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushMongoContextSettings));

            // Act
            // Assert
            await sut.UpsertAsync(domain);
        }
    }
}
