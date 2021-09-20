using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public class DeviceTokenValidator : IDeviceTokenValidator
    {
        private readonly DeviceTokenValidatorSettings _deviceTokenValidatorSettings;

        public DeviceTokenValidator(IOptions<DeviceTokenValidatorSettings> deviceTokenValidatorSettings)
        {
            _deviceTokenValidatorSettings = deviceTokenValidatorSettings.Value;
        }

        public async Task<bool> IsValidAsync(string deviceToken)
        {
            var responseBody = await _deviceTokenValidatorSettings.PushApiUrl
                        .AppendPathSegment($"devices/{deviceToken}")
                        .GetJsonAsync();

            return responseBody.isValid;
        }
    }
}
