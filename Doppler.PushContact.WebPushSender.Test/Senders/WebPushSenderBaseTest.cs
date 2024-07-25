using AutoFixture;
using Doppler.PushContact.Models.DTOs;
using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs.WebPushApi;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Senders;
using Doppler.PushContact.WebPushSender.Test.Senders.Dummies;
using Flurl.Http.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.WebPushSender.Test.Senders
{
    public class WebPushSenderBaseTests
    {
        private static readonly WebPushSenderSettings webPushSenderSettingsDefault =
            new WebPushSenderSettings
            {
                PushApiUrl = "https://api.push.com",
                QueueName = "test.queue"
            };

        private static TestWebPushSender CreateSUT(
            IOptions<WebPushSenderSettings> webPushSenderSettings = null,
            IMessageQueueSubscriber messageQueueSubscriber = null,
            ILogger<TestWebPushSender> logger = null,
            IWebPushEventRepository webPushEventRepository = null)
        {
            return new TestWebPushSender(
                webPushSenderSettings ?? Options.Create(webPushSenderSettingsDefault),
                messageQueueSubscriber ?? Mock.Of<IMessageQueueSubscriber>(),
                logger ?? Mock.Of<ILogger<TestWebPushSender>>(),
                webPushEventRepository ?? Mock.Of<IWebPushEventRepository>()
            );
        }

        private DopplerWebPushDTO GetMessage(string title, string body, string endpoint, string auth, string p256dh)
        {
            return new DopplerWebPushDTO
            {
                Subscription = new SubscriptionDTO
                {
                    EndPoint = endpoint,
                    Keys = new SubscriptionKeys
                    {
                        P256DH = p256dh,
                        Auth = auth
                    }
                },
                Title = title,
                Body = body,
            };
        }

        [Fact]
        public async Task SendWebPush_Should_Return_SuccessfullyDelivered_When_IsSuccess_True()
        {
            // Arrange
            var fixture = new Fixture();

            var endpoint = fixture.Create<string>();

            var message = GetMessage(
                fixture.Create<string>(),
                fixture.Create<string>(),
                endpoint,
                fixture.Create<string>(),
                fixture.Create<string>()
            );

            var sendMessageResponseExpected = new SendMessageResponse
            {
                Responses = new List<SendMessageResponseDetail>
                {
                    new SendMessageResponseDetail
                    {
                        IsSuccess = true,
                        Exception = null,
                        Subscription = new SubscriptionResponse
                        {
                            Endpoint = endpoint
                        }
                    }
                }
            };

            var webPushSender = CreateSUT();

            // mock the HTTP response
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(sendMessageResponseExpected);

                // Act
                var processingResult = await webPushSender.TestSendWebPush(message);

                // Assert
                Assert.False(processingResult.FailedProcessing);
                Assert.True(processingResult.SuccessfullyDelivered);
                Assert.False(processingResult.LimitsExceeded);
                Assert.False(processingResult.InvalidSubscription);
            }
        }

        [Fact]
        public async Task SendWebPush_Should_Return_LimitsExceeded_When_IsSuccess_False_And_MessagingErrorCode_Is_429()
        {
            // Arrange
            var fixture = new Fixture();

            var endpoint = fixture.Create<string>();

            var message = GetMessage(
                fixture.Create<string>(),
                fixture.Create<string>(),
                endpoint,
                fixture.Create<string>(),
                fixture.Create<string>()
            );

            var sendMessageResponseExpected = new SendMessageResponse
            {
                Responses = new List<SendMessageResponseDetail>
                {
                    new SendMessageResponseDetail
                    {
                        IsSuccess = false,
                        Exception = new SendMessageResponseException
                        {
                            MessagingErrorCode = 429,
                        },
                        Subscription = new SubscriptionResponse
                        {
                            Endpoint = endpoint
                        }
                    }
                }
            };

            var webPushSender = CreateSUT();

            // mock the HTTP response
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(sendMessageResponseExpected);

                // Act
                var processingResult = await webPushSender.TestSendWebPush(message);

                // Assert
                Assert.False(processingResult.FailedProcessing);
                Assert.False(processingResult.SuccessfullyDelivered);
                Assert.True(processingResult.LimitsExceeded);
                Assert.False(processingResult.InvalidSubscription);
            }
        }

        [Theory]
        [InlineData((int)HttpStatusCode.NotFound)]
        [InlineData((int)HttpStatusCode.Gone)]
        public async Task SendWebPush_Should_Return_InvalidSubscription_When_IsSuccess_False_And_MessagingErrorCode_Is_NotFound_Or_Gone(
            int messagingErrorCode
        )
        {
            // Arrange
            var fixture = new Fixture();

            var endpoint = fixture.Create<string>();

            var message = GetMessage(
                fixture.Create<string>(),
                fixture.Create<string>(),
                endpoint,
                fixture.Create<string>(),
                fixture.Create<string>()
            );

            var sendMessageResponseExpected = new SendMessageResponse
            {
                Responses = new List<SendMessageResponseDetail>
                {
                    new SendMessageResponseDetail
                    {
                        IsSuccess = false,
                        Exception = new SendMessageResponseException
                        {
                            MessagingErrorCode = messagingErrorCode,
                        },
                        Subscription = new SubscriptionResponse
                        {
                            Endpoint = endpoint
                        }
                    }
                }
            };

            var webPushSender = CreateSUT();

            // mock the HTTP response
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWithJson(sendMessageResponseExpected);

                // Act
                var processingResult = await webPushSender.TestSendWebPush(message);

                // Assert
                Assert.False(processingResult.FailedProcessing);
                Assert.False(processingResult.SuccessfullyDelivered);
                Assert.False(processingResult.LimitsExceeded);
                Assert.True(processingResult.InvalidSubscription);
            }
        }

        [Fact]
        public async Task SendWebPush_Should_Return_FailedProcessing_When_PushApi_Throws_Exception()
        {
            // Arrange
            var fixture = new Fixture();

            var message = GetMessage(
                fixture.Create<string>(),
                fixture.Create<string>(),
                fixture.Create<string>(),
                fixture.Create<string>(),
                fixture.Create<string>()
            );

            var mockLogger = new Mock<ILogger<TestWebPushSender>>();

            var webPushSender = CreateSUT(
                logger: mockLogger.Object
            );

            // mock the HTTP response
            using (var httpTest = new HttpTest())
            {
                httpTest.RespondWith(status: 500);

                // Act
                var processingResult = await webPushSender.TestSendWebPush(message);

                // Assert
                Assert.True(processingResult.FailedProcessing);
                Assert.False(processingResult.SuccessfullyDelivered);
                Assert.False(processingResult.LimitsExceeded);
                Assert.False(processingResult.InvalidSubscription);

                mockLogger.Verify(
                    x => x.Log(
                        It.Is<LogLevel>(l => l == LogLevel.Error),
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error happened sending web push notification")),
                        It.IsAny<Exception>(),
                        It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                    Times.Once);
            }
        }
    }
}
