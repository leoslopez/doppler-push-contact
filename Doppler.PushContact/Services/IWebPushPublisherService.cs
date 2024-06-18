using Doppler.PushContact.DTOs;
namespace Doppler.PushContact.Services
{
    public interface IWebPushPublisherService
    {
        void ProcessWebPush(string domain, WebPushDTO webPushDTO, string authenticationApiToken = null);
    }
}
