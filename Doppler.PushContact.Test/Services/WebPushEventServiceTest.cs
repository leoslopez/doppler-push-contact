using AutoFixture;
using Doppler.PushContact.DTOs;
using Doppler.PushContact.Repositories.Interfaces;
using Doppler.PushContact.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.Test.Services
{
    public class WebPushEventServiceTest
    {
        private static WebPushEventService CreateSut(
            IWebPushEventRepository webPushEventRepository = null,
            ILogger<WebPushEventService> logger = null)
        {
            return new WebPushEventService(
                webPushEventRepository ?? Mock.Of<IWebPushEventRepository>(),
                logger ?? Mock.Of<ILogger<WebPushEventService>>()
            );
        }

        [Fact]
        public async Task GetWebPushEventSummarizationAsync_should_log_error_and_return_empty_stats_when_repository_throw_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var messageId = fixture.Create<Guid>();
            var expectedMessageException = $"Error summarizing 'WebPushEvents' with {nameof(messageId)} {messageId}";

            var mockRepository = new Mock<IWebPushEventRepository>();
            var mockLogger = new Mock<ILogger<WebPushEventService>>();

            mockRepository
                .Setup(repo => repo.GetWebPushEventSummarization(messageId))
                .ThrowsAsync(new Exception("Repository exception"));

            var sut = CreateSut(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await sut.GetWebPushEventSummarizationAsync(messageId);

            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessageException)),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()
                ),
                Times.Once
            );

            Assert.NotNull(result);
            Assert.Equal(messageId, result.MessageId);
            Assert.Equal(0, result.SentQuantity);
            Assert.Equal(0, result.Delivered);
            Assert.Equal(0, result.NotDelivered);
        }

        [Fact]
        public async Task GetWebPushEventSummarizationAsync_should_return_stats_ok()
        {
            // Arrange
            var fixture = new Fixture();

            var messageId = fixture.Create<Guid>();

            var mockRepository = new Mock<IWebPushEventRepository>();
            var mockLogger = new Mock<ILogger<WebPushEventService>>();

            var expectedSummarization = new WebPushEventSummarizationDTO
            {
                MessageId = messageId,
                SentQuantity = 10,
                Delivered = 8,
                NotDelivered = 2
            };

            mockRepository
                .Setup(repo => repo.GetWebPushEventSummarization(messageId))
                .ReturnsAsync(expectedSummarization);

            var sut = CreateSut(mockRepository.Object, mockLogger.Object);

            // Act
            var result = await sut.GetWebPushEventSummarizationAsync(messageId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSummarization.MessageId, result.MessageId);
            Assert.Equal(expectedSummarization.SentQuantity, result.SentQuantity);
            Assert.Equal(expectedSummarization.Delivered, result.Delivered);
            Assert.Equal(expectedSummarization.NotDelivered, result.NotDelivered);
        }
    }
}
