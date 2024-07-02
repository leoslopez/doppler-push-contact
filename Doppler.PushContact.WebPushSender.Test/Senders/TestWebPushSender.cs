using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Test.Senders
{
    public class TestWebPushSender : WebPushSenderBase
    {
        public TestWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger<TestWebPushSender> logger
        ) : base(webPushSenderSettings, messageQueueSubscriber, logger)
        {
        }

        public override Task HandleMessageAsync(DopplerWebPushDTO message)
        {
            throw new NotImplementedException();
        }

        public async Task<WebPushProcessingResult> TestSendWebPush(DopplerWebPushDTO message)
        {
            return await SendWebPush(message);
        }
    }

}
