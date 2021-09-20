using AutoFixture;
using Doppler.PushContact.Services;
using Flurl.Http;
using Flurl.Http.Testing;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.PushContact.Test.Services
{
    public class DeviceTokenValidatorTest
    {
        private static readonly DeviceTokenValidatorSettings deviceTokenValidatorSettingsDefault =
            new DeviceTokenValidatorSettings
            {
                PushApiUrl = "https://localhost:9999"
            };

        private static DeviceTokenValidator CreateSut(
            IOptions<DeviceTokenValidatorSettings> deviceTokenValidatorSettings = null)
        {
            return new DeviceTokenValidator(
                deviceTokenValidatorSettings ?? Options.Create(deviceTokenValidatorSettingsDefault));
        }

        [Fact]
        public async Task IsValidAsync_should_return_is_valid_push_api_response()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokenToBeValidated = fixture.Create<string>();
            var isValidPushApiResponse = fixture.Create<bool>();

            using var httpTest = new HttpTest();

            httpTest.RespondWithJson(new { isValid = isValidPushApiResponse }, 200);

            var sut = CreateSut();

            // Act
            var isValidResult = await sut.IsValidAsync(deviceTokenToBeValidated);

            // Assert
            Assert.Equal(isValidPushApiResponse, isValidResult);
            httpTest.ShouldHaveCalled($"{deviceTokenValidatorSettingsDefault.PushApiUrl}/devices/{deviceTokenToBeValidated}")
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        [Theory]
        [InlineData(500)]
        [InlineData(400)]
        [InlineData(401)]
        [InlineData(404)]
        public async Task IsValidAsync_should_throw_flurl_http_exception_when_push_api_response_a_not_success_status(int notSuccessStatus)
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokenToBeValidated = fixture.Create<string>();

            using var httpTest = new HttpTest();

            httpTest.RespondWithJson(string.Empty, notSuccessStatus);

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<FlurlHttpException>(() => sut.IsValidAsync(deviceTokenToBeValidated));
            httpTest.ShouldHaveCalled($"{deviceTokenValidatorSettingsDefault.PushApiUrl}/devices/{deviceTokenToBeValidated}")
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        [Fact]
        public async Task IsValidAsync_should_throw_runtime_binder_exception_when_push_api_get_device_is_valid_key_contract_was_changed()
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokenToBeValidated = fixture.Create<string>();

            using var httpTest = new HttpTest();

            httpTest.RespondWithJson(new { isValidKeyContractChanged = fixture.Create<bool>() }, 200);

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<RuntimeBinderException>(() => sut.IsValidAsync(deviceTokenToBeValidated));
            httpTest.ShouldHaveCalled($"{deviceTokenValidatorSettingsDefault.PushApiUrl}/devices/{deviceTokenToBeValidated}")
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }

        [Theory]
        [InlineData("true")]
        [InlineData(1)]
        public async Task
            IsValidAsync_should_throw_runtime_binder_exception_when_push_api_get_device_is_valid_value_type_contract_was_changed(object isValidValueContractChanged)
        {
            // Arrange
            var fixture = new Fixture();

            var deviceTokenToBeValidated = fixture.Create<string>();

            using var httpTest = new HttpTest();

            httpTest.RespondWithJson(new { isValid = isValidValueContractChanged }, 200);

            var sut = CreateSut();

            // Act
            // Assert
            await Assert.ThrowsAsync<RuntimeBinderException>(() => sut.IsValidAsync(deviceTokenToBeValidated));
            httpTest.ShouldHaveCalled($"{deviceTokenValidatorSettingsDefault.PushApiUrl}/devices/{deviceTokenToBeValidated}")
                .WithVerb(HttpMethod.Get)
                .Times(1);
        }
    }
}
