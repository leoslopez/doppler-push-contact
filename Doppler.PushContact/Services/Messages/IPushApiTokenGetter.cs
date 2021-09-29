using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public interface IPushApiTokenGetter
    {
        Task<string> GetTokenAsync();
    }
}
