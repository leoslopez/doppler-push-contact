using Microsoft.Extensions.Options;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public interface IWebPushSenderFactory
    {
        IWebPushSender CreateSender(IOptions<WebPushSenderSettings> webPushSenderSettings);
    }

}
