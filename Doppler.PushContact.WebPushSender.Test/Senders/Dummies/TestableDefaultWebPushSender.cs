using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Test.Senders.Dummies
{
    // Define delegate
    public delegate Task<WebPushProcessingResult> SendWebPushDelegate(DopplerWebPushDTO message);

    public class TestableDefaultWebPushSender : DefaultWebPushSender
    {
        private readonly SendWebPushDelegate _sendWebPushDelegate;

        public TestableDefaultWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger<DefaultWebPushSender> logger,
            IWebPushEventRepository weshPushEventRepository,
            SendWebPushDelegate sendWebPushDelegate)
            : base(webPushSenderSettings, messageQueueSubscriber, logger, weshPushEventRepository)
        {
            _sendWebPushDelegate = sendWebPushDelegate;
        }

        protected override Task<WebPushProcessingResult> SendWebPush(DopplerWebPushDTO message)
        {
            return _sendWebPushDelegate(message);
        }
    }
}
