using AutoFixture;
using Doppler.PushContact.WebPushSender.Repositories;
using Doppler.PushContact.WebPushSender.Repositories.Setup;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.WebPushSender.Test.Repositories
{
    public class PushContactRepositoryTest
    {
        private readonly Mock<IMongoCollection<BsonDocument>> _mockCollection;
        private readonly Mock<IOptions<RepositorySettings>> _mockSettings;
        private readonly Mock<ILogger<PushContactRepository>> _mockLogger;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly PushContactRepository _repository;

        public PushContactRepositoryTest()
        {
            _mockCollection = new Mock<IMongoCollection<BsonDocument>>();
            _mockSettings = new Mock<IOptions<RepositorySettings>>();
            _mockLogger = new Mock<ILogger<PushContactRepository>>();
            _mockDatabase = new Mock<IMongoDatabase>();

            _mockSettings.Setup(s => s.Value).Returns(new RepositorySettings
            {
                PushContactsCollectionName = "pushContacts"
            });

            _mockDatabase.Setup(d => d.GetCollection<BsonDocument>("pushContacts", null))
                .Returns(_mockCollection.Object);

            _repository = new PushContactRepository(_mockDatabase.Object, _mockSettings.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task MarkDeletedAsync_ShouldReturnTrue_WhenDocumentIsUpdated()
        {
            // Arrange
            var fixture = new Fixture();
            var pushContactId = fixture.Create<string>();

            var updateResultExpected = new UpdateResult.Acknowledged(1, 1, new BsonObjectId(ObjectId.GenerateNewId()));

            _mockCollection
                .Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateOptions>(),
                                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultExpected);

            // Act
            var result = await _repository.MarkDeletedAsync(pushContactId);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task MarkDeletedAsync_ShouldReturnFalse_WhenDocumentIsNotUpdated()
        {
            // Arrange
            var fixture = new Fixture();
            var pushContactId = fixture.Create<string>();

            var updateResultExpected = new UpdateResult.Acknowledged(0, 0, new BsonObjectId(ObjectId.GenerateNewId()));

            _mockCollection
                .Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateOptions>(),
                                            It.IsAny<CancellationToken>()))
                .ReturnsAsync(updateResultExpected);

            // Act
            var result = await _repository.MarkDeletedAsync(pushContactId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MarkDeletedAsync_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var fixture = new Fixture();
            var pushContactId = fixture.Create<string>();

            _mockCollection
                .Setup(c => c.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateDefinition<BsonDocument>>(),
                                            It.IsAny<UpdateOptions>(),
                                            It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await _repository.MarkDeletedAsync(pushContactId);

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error marking PushContact deleted")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
