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
        private const string PushContactDocumentIdPropName = "_id";
        private const string PushContactDocumentDomainPropName = "domain";
        private const string PushContactDocumentDeviceTokenPropName = "device_token";
        private const string PushContactDocumentEmailPropName = "email";
        private const string PushContactDocumentDeletedPropName = "deleted";
        private const string PushContactDocumentModifiedPropName = "modified";

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
        public async Task AddAsync_should_throw_exception_and_log_error_when_a_push_contact_model_cannot_be_added()
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
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.AddAsync(pushContactModel));

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
        public async Task AddAsync_should_not_throw_exception_when_a_push_contact_model_can_be_added()
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
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            // Assert
            await sut.AddAsync(pushContactModel);
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
        [InlineData("someDeviceToken", null)]
        [InlineData(null, "someEmail")]
        [InlineData(null, null)]
        public async Task UpdateEmailAsync_should_throw_argument_null_exception_when_device_token_or_email_are_null(string deviceToken, string email)
        {
            // Arrange
            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => sut.UpdateEmailAsync(deviceToken, email));
        }

        [Fact]
        public async Task UpdateEmailAsync_should_throw_exception_and_log_error_when_push_contact_model_cannot_be_updated()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), default, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.UpdateEmailAsync(deviceToken, email));
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == @$"Error updating {nameof(PushContactModel)}
with {nameof(deviceToken)} {deviceToken}. {PushContactDocumentEmailPropName} can not be updated with following value: {email}"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task UpdateEmailAsync_should_not_throw_exception_when_push_contact_model_can_be_updated()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceToken = fixture.Create<string>();
            var email = fixture.Create<string>();

            var updateResultMock = new Mock<UpdateResult>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollection = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollection
                .Setup(x => x.UpdateOneAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), default, default))
                .ReturnsAsync(updateResultMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            // Assert
            await sut.UpdateEmailAsync(deviceToken, email);
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
        public async Task
            GetAsync_should_throw_argument_exception_when_push_contact_filter_modified_from_is_greater_than_modified_to()
        {
            // Arrange
            var fixture = new Fixture();

            var domain = fixture.Create<string>();
            var modifiedFrom = fixture.Create<DateTime>();
            var modifiedTo = modifiedFrom.AddDays(-1);

            var pushContactFilter = new PushContactFilter(domain: domain, modifiedFrom: modifiedFrom, modifiedTo: modifiedTo);

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.GetAsync(pushContactFilter));
            Assert.Equal($"'{nameof(pushContactFilter.ModifiedFrom)}' cannot be greater than '{nameof(pushContactFilter.ModifiedTo)}'", result.Message);
        }

        [Fact]
        public async Task GetAsync_should_throw_exception_and_log_error_when_push_contacts_cannot_be_getter()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var domain = fixture.Create<string>();
            var pushContactFilter = new PushContactFilter(domain);

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.GetAsync(pushContactFilter));

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
            var domainFilter = allPushContactDocuments[randomPushContactIndex][PushContactDocumentDomainPropName].AsString;
            var pushContactFilter = new PushContactFilter(domainFilter);

            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            pushContactsCursorMock
                .Setup(_ => _.Current)
                .Returns(allPushContactDocuments.Where(x => x[PushContactDocumentDomainPropName].AsString == pushContactFilter.Domain));

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(pushContactsCursorMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            Assert.True(result.Any());
            Assert.True(result.All(x => x.Domain == pushContactFilter.Domain));
        }

        [Fact]
        public async Task GetAsync_should_return_push_contacts_filtered_by_domain_and_email()
        {
            // Arrange
            List<BsonDocument> allPushContactDocuments = FakePushContactDocuments(10);

            var random = new Random();
            int randomPushContactIndex = random.Next(allPushContactDocuments.Count);
            var domainFilter = allPushContactDocuments[randomPushContactIndex][PushContactDocumentDomainPropName].AsString;
            var emailFilter = allPushContactDocuments[randomPushContactIndex][PushContactDocumentEmailPropName].AsString;
            var pushContactFilter = new PushContactFilter(domainFilter, emailFilter);

            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            pushContactsCursorMock
                .Setup(_ => _.Current)
                .Returns(allPushContactDocuments.Where(x => x[PushContactDocumentDomainPropName].AsString == pushContactFilter.Domain && x[PushContactDocumentEmailPropName].AsString == pushContactFilter.Email));

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(pushContactsCursorMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            Assert.True(result.Any());
            Assert.True(result.All(x => x.Domain == pushContactFilter.Domain && x.Email == pushContactFilter.Email));
        }

        [Fact]
        public async Task GetAsync_should_return_not_deleted_push_contacts()
        {
            // Arrange
            List<BsonDocument> allPushContactDocuments = FakePushContactDocuments(10);
            var random = new Random();
            int randomPushContactIndex = random.Next(allPushContactDocuments.Count);
            allPushContactDocuments[randomPushContactIndex][PushContactDocumentDeletedPropName] = false;

            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            pushContactsCursorMock
                .Setup(_ => _.Current)
                .Returns(allPushContactDocuments.Where(x => x[PushContactDocumentDeletedPropName].AsBoolean == false));

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(pushContactsCursorMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            var domain = fixture.Create<string>();
            var pushContactFilter = new PushContactFilter(domain);

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            Assert.True(result.Any());
            Assert.True(result.All(x => allPushContactDocuments.Exists(y => y[PushContactDocumentDomainPropName] == x.Domain
                                                                            && y[PushContactDocumentDeviceTokenPropName] == x.DeviceToken
                                                                            && y[PushContactDocumentEmailPropName] == x.Email
                                                                            && y[PushContactDocumentDeletedPropName].AsBoolean == false)));
        }

        [Fact]
        public async Task GetAsync_should_not_throw_exceptions_when_push_contacts_documents_have_not_some_property()
        {
            // Arrange
            List<BsonDocument> pushContactDocumentsWithoutProperties = Enumerable.Repeat(0, 10)
                .Select(x =>
                {
                    return new BsonDocument { };
                })
                .ToList();

            var fixture = new Fixture();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCursorMock = new Mock<IAsyncCursor<BsonDocument>>();
            pushContactsCursorMock
                .Setup(_ => _.Current)
                .Returns(pushContactDocumentsWithoutProperties);

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            pushContactsCursorMock
                .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(true))
                .Returns(Task.FromResult(false));

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.FindAsync<BsonDocument>(It.IsAny<FilterDefinition<BsonDocument>>(), null, default))
                .ReturnsAsync(pushContactsCursorMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            var domain = fixture.Create<string>();
            var pushContactFilter = new PushContactFilter(domain);

            // Act
            var result = await sut.GetAsync(pushContactFilter);

            // Assert
            _ = result.ToList();
        }

        [Fact]
        public async Task DeleteByDeviceTokenAsync_should_throw_argument_exception_when_device_tokens_collection_is_null()
        {
            // Arrange
            IEnumerable<string> deviceTokens = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.DeleteByDeviceTokenAsync(deviceTokens));
        }

        [Fact]
        public async Task DeleteByDeviceTokenAsync_should_throw_argument_exception_when_device_tokens_collection_is_empty()
        {
            // Arrange
            IEnumerable<string> deviceTokens = new List<string>();

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.DeleteByDeviceTokenAsync(deviceTokens));
        }

        [Fact]
        public async Task DeleteByDeviceTokenAsync_should_throw_exception_and_log_error_when_push_contact_models_cannot_be_deleted()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokens = fixture.CreateMany<string>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.UpdateManyAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), default, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.DeleteByDeviceTokenAsync(deviceTokens));
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Error deleting {nameof(PushContactModel)}s"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task DeleteByDeviceTokenAsync_should_return_deleted_count_when_push_contact_models_can_be_deleted()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokens = fixture.CreateMany<string>();
            var expectedDeletedCount = fixture.Create<long>();

            var updateResultMock = new Mock<UpdateResult>();
            updateResultMock.Setup(_ => _.IsModifiedCountAvailable).Returns(true);
            updateResultMock.Setup(_ => _.ModifiedCount).Returns(expectedDeletedCount);

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollection = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollection
                .Setup(x => x.UpdateManyAsync(It.IsAny<FilterDefinition<BsonDocument>>(), It.IsAny<UpdateDefinition<BsonDocument>>(), default, default))
                .ReturnsAsync(updateResultMock.Object);

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings));

            // Act
            var deletedCount = await sut.DeleteByDeviceTokenAsync(deviceTokens);

            // Assert
            Assert.Equal(expectedDeletedCount, updateResultMock.Object.ModifiedCount);
        }

        [Fact]
        public async Task AddHistoryEventsAsync_should_throw_argument_exception_when_push_contact_history_events_collection_is_null()
        {
            // Arrange
            IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents = null;

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.AddHistoryEventsAsync(pushContactHistoryEvents));
        }

        [Fact]
        public async Task AddHistoryEventsAsync_should_throw_argument_exception_when_push_contact_history_events_collection_is_empty()
        {
            // Arrange
            IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents = new List<PushContactHistoryEvent>();

            var sut = CreateSut();

            // Act
            // Assert
            var result = await Assert.ThrowsAsync<ArgumentException>(() => sut.AddHistoryEventsAsync(pushContactHistoryEvents));
        }

        [Fact]
        public async Task AddHistoryEventsAsync_should_throw_exception_and_log_error_when_push_contact_history_events_cannot_be_added()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactHistoryEvents = fixture.CreateMany<PushContactHistoryEvent>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollectionMock = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollectionMock
                .Setup(x => x.BulkWriteAsync(It.IsAny<IEnumerable<UpdateOneModel<BsonDocument>>>(), default, default))
                .ThrowsAsync(new Exception());

            var mongoDatabaseMock = new Mock<IMongoDatabase>();
            mongoDatabaseMock
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollectionMock.Object);

            var mongoClientMock = new Mock<IMongoClient>();
            mongoClientMock
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabaseMock.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClientMock.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.AddHistoryEventsAsync(pushContactHistoryEvents));
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Error adding {nameof(PushContactHistoryEvent)}s"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task AddHistoryEventsAsync_should_not_throw_exception_and_not_log_error_when_push_contact_history_events_can_be_added()
        {
            // Arrange
            var fixture = new Fixture();

            var pushContactHistoryEvents = fixture.CreateMany<PushContactHistoryEvent>();

            var pushContactMongoContextSettings = fixture.Create<PushContactMongoContextSettings>();

            var pushContactsCollection = new Mock<IMongoCollection<BsonDocument>>();
            pushContactsCollection
                .Setup(x => x.BulkWriteAsync(It.IsAny<IEnumerable<UpdateOneModel<BsonDocument>>>(), default, default))
                .Returns(Task.FromResult(It.IsAny<BulkWriteResult<BsonDocument>>()));

            var mongoDatabase = new Mock<IMongoDatabase>();
            mongoDatabase
                .Setup(x => x.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName, null))
                .Returns(pushContactsCollection.Object);

            var mongoClient = new Mock<IMongoClient>();
            mongoClient
                .Setup(x => x.GetDatabase(pushContactMongoContextSettings.DatabaseName, null))
                .Returns(mongoDatabase.Object);

            var loggerMock = new Mock<ILogger<PushContactService>>();

            var sut = CreateSut(
                mongoClient.Object,
                Options.Create(pushContactMongoContextSettings),
                loggerMock.Object);

            // Act
            await sut.AddHistoryEventsAsync(pushContactHistoryEvents);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Error adding {nameof(PushContactHistoryEvent)}s"),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Never);
        }

        private static List<BsonDocument> FakePushContactDocuments(int count)
        {
            var fixture = new Fixture();

            return Enumerable.Repeat(0, count)
                .Select(x =>
                {
                    return new BsonDocument {
                            { PushContactDocumentIdPropName, fixture.Create<string>() },
                            { PushContactDocumentDomainPropName, fixture.Create<string>() },
                            { PushContactDocumentDeviceTokenPropName, fixture.Create<string>() },
                            { PushContactDocumentEmailPropName, fixture.Create<string>() },
                            { PushContactDocumentModifiedPropName, fixture.Create<DateTime>().ToUniversalTime() },
                            { PushContactDocumentDeletedPropName, fixture.Create<bool>() }
                    };
                })
                .ToList();
        }
    }
}
