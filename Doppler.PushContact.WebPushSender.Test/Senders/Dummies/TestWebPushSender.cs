using Doppler.PushContact.Models.DTOs;
using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Senders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Test.Senders.Dummies
{
    public class TestWebPushSender : WebPushSenderBase
    {
        public TestWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger<TestWebPushSender> logger,
            IWebPushEventRepository webPushEventRepository
        ) : base(webPushSenderSettings, messageQueueSubscriber, logger, webPushEventRepository)
        {
        }

        public override Task HandleMessageAsync(DopplerWebPushDTO message)
        {
            throw new NotImplementedException();
        }

        public async Task<WebPushProcessingResultDTO> TestSendWebPush(DopplerWebPushDTO message)
        {
            return await SendWebPush(message);
        }
    }

}
