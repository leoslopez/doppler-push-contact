using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IDeviceTokenValidator
    {
        Task<bool> IsValidAsync(string deviceToken);
    }
}
