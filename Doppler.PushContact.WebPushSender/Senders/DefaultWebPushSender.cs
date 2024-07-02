using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public class DefaultWebPushSender : WebPushSenderBase
    {
        public DefaultWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger<DefaultWebPushSender> logger
        )
            : base(webPushSenderSettings, messageQueueSubscriber, logger)
        {
        }

        public override async Task HandleMessageAsync(DopplerWebPushDTO message)
        {
            _logger.LogDebug(
                "Processing message in \"{QueueName}\":\n\tEndpoint: {EndPoint}",
                _queueName,
                message.Subscription.EndPoint
            );

            WebPushProcessingResult processingResult = await SendWebPush(message);

            _logger.LogDebug(
                "Message processed:\n\tEndpoint: {EndPoint}\n\tResult: {WebPushProcessingResult}",
                message.Subscription.EndPoint,
                JsonConvert.SerializeObject(processingResult)
            );

            // TODO: analyze processingResult and take proper actions (register in db, retry, etc)
        }
    }
}
