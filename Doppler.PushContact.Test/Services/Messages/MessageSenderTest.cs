using AutoFixture;
using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Services.Messages.ExternalContracts;
using Flurl.Http;
using Flurl.Http.Testing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.Test.Services.Messages
{
    public class MessageSenderTest
    {
        private static readonly MessageSenderSettings messageSenderSettingsDefault =
            new MessageSenderSettings
            {
                PushApiUrl = "https://localhost:9999",
                FatalMessagingErrorCodes = new List<int>() { 1, 2, 3, 4 }
            };

        private static MessageSender CreateSut(
            IPushApiTokenGetter pushApiTokenGetter = null,
            IOptions<MessageSenderSettings> messageSenderSettings = null)
        {
            return new MessageSender(
                messageSenderSettings ?? Options.Create(messageSenderSettingsDefault),
                pushApiTokenGetter ?? Mock.Of<IPushApiTokenGetter>());
        }

        [Theory]
        [InlineData(null, "some body", new string[] { "someTargetDeviceTokens" })]
        [InlineData("some title", null, new string[] { "someTargetDeviceTokens" })]
        [InlineData("some title", "some body", null)]
        [InlineData(null, null, null)]
        [InlineData("some title", "", new string[] { "someTargetDeviceTokens" })]
        [InlineData("", "some body", new string[] { "someTargetDeviceTokens" })]
        [InlineData("some title", "some body", new string[] { })]
        [InlineData("", "", new string[] { })]
        public async Task
            SendAsync_should_throw_argument_exception_and_not_have_made_a_http_call_when_title_or_body_or_target_are_null_or_empty(string title, string body, string[] targetDeviceTokens)
        {
            // Arrange
            using var httpTest = new HttpTest();

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => sut.SendAsync(title, body, targetDeviceTokens));
            httpTest.ShouldNotHaveMadeACall();
        }

        [Theory]
        [InlineData(500)]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(404)]
        public async Task SendAsync_should_throw_flurl_http_exception_when_push_api_response_a_not_success_status(int notSuccessStatus)
        {
            // Arrange
            var fixture = new Fixture();

            var title = fixture.Create<string>();
            var body = fixture.Create<string>();
            var targetDeviceTokens = fixture.CreateMany<string>();

            using var httpTest = new HttpTest();

            httpTest.RespondWithJson(string.Empty, notSuccessStatus);

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<FlurlHttpException>(() => sut.SendAsync(title, body, targetDeviceTokens));
            httpTest.ShouldHaveCalled($"{messageSenderSettingsDefault.PushApiUrl}/message")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Fact]
        public async Task SendAsync_should_throw_exception_and_does_not_call_push_api_when_push_api_token_getter_throw_exception()
        {
            // Arrange
            var fixture = new Fixture();

            var title = fixture.Create<string>();
            var body = fixture.Create<string>();
            var targetDeviceTokens = fixture.CreateMany<string>();

            using var httpTest = new HttpTest();

            var pushApiTokenGetterMock = new Mock<IPushApiTokenGetter>();
            pushApiTokenGetterMock
                .Setup(x => x.GetTokenAsync())
                .ThrowsAsync(new Exception());

            var sut = CreateSut(
                pushApiTokenGetter: pushApiTokenGetterMock.Object
                );

            // Act
            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.SendAsync(title, body, targetDeviceTokens));
            httpTest.ShouldNotHaveMadeACall();
        }

        [Fact]
        public async Task SendAsync_should_return_only_one_message_target_result_per_push_api_send_message_response()
        {
            // Arrange
            var fixture = new Fixture();

            var title = fixture.Create<string>();
            var body = fixture.Create<string>();
            var targetDeviceTokens = fixture.CreateMany<string>();

            var sendMessageResponse = fixture.Create<SendMessageResponse>();

            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(sendMessageResponse, 200);

            var sut = CreateSut();

            // Act
            var sendMessageResult = await sut.SendAsync(title, body, targetDeviceTokens);

            // Assert
            Assert.All(sendMessageResponse.Responses, x => sendMessageResult.SendMessageTargetResult.Single(y => y.TargetDeviceToken == x.DeviceToken));
            httpTest.ShouldHaveCalled($"{messageSenderSettingsDefault.PushApiUrl}/message")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }

        [Theory]
        [InlineData("http://urlwithhttpschema.com/random-resource.img")]
        [InlineData("urlwithoutschema.com/random-resource.img")]
        [InlineData("//not/absolute/url/random-resource.img")]
        [InlineData("https:invalidurl.com/random-resource.img")]
        [InlineData("https://invalidurl.com<>/random-resource.img")]
        public async Task
            SendAsync_should_throw_argument_exception_and_not_have_made_a_http_call_when_onClickLink_is_a_not_valid_url(string notValidOnClickLink)
        {
            // Arrange
            var fixture = new Fixture();

            var title = fixture.Create<string>();
            var body = fixture.Create<string>();
            var targetDeviceTokens = fixture.CreateMany<string>();

            using var httpTest = new HttpTest();

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<ArgumentException>(() => sut.SendAsync(title, body, targetDeviceTokens, notValidOnClickLink));
            httpTest.ShouldNotHaveMadeACall();
        }

        [Theory]
        [InlineData("https://valid-domain.com/")]
        [InlineData("https://valid-domain.com")]
        [InlineData("https://valid-domain.com/random-resource.img")]
        [InlineData("https://www.valid-domain.com/random-resource.img")]
        [InlineData("https://valid-domain.com/random-resource.img?someParam=paramValue&otherParam=otherValue")]
        [InlineData("https://valid-domain.com/random-resource.img#L6")]
        public async Task
            SendAsync_should_does_not_throw_exception_and_have_made_a_http_call_when_all_params_are_valid(string validOnClickLink)
        {
            // Arrange
            var fixture = new Fixture();

            var validTitle = "Valid title";
            var validBody = "Valid body";
            var validTargetDeviceTokens = new string[] { "someTargetDeviceToken" };

            var sendMessageResponse = fixture.Create<SendMessageResponse>();

            using var httpTest = new HttpTest();
            httpTest.RespondWithJson(sendMessageResponse, 200);

            var sut = CreateSut();

            // Act
            await sut.SendAsync(validTitle, validBody, validTargetDeviceTokens, validOnClickLink);

            // Assert
            httpTest.ShouldHaveCalled($"{messageSenderSettingsDefault.PushApiUrl}/message")
                .WithVerb(HttpMethod.Post)
                .Times(1);
        }
    }
}
