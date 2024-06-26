using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public class DefaultWebPushSender : WebPushSenderBase
    {
        public DefaultWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueConsumer,
            ILogger<DefaultWebPushSender> logger
        )
            : base(webPushSenderSettings, messageQueueConsumer, logger)
        {
        }

        public override async Task HandleMessageAsync(DopplerWebPushDTO message)
        {
            // TODO: consider specific implementation
            _logger.LogInformation(
                "Process message in \"{queueName}\":\n\tEndpoint: {EndPoint}\n\tMessageId: {MessageId}",
                _queueName,
                message.Subscription.EndPoint,
                message.MessageId
                );
            await Task.Delay(2000);
        }
    }
}
