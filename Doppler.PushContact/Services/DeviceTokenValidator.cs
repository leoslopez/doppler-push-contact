using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public class DeviceTokenValidator : IDeviceTokenValidator
    {
        public async Task<bool> IsValidAsync(string token)
        public async Task<bool> IsValidAsync(string deviceToken)
        {
            throw new NotImplementedException();
        }
    }
}
