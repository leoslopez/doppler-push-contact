using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public class DefaultWebPushSender : WebPushSenderBase
    {
        // TODO: obtain queueName from config file
        public DefaultWebPushSender(IMessageQueueSubscriber messageQueueConsumer, ILogger<DefaultWebPushSender> logger)
            : base("default.webpush.queue", messageQueueConsumer, logger)
        {
        }

        public override async Task HandleMessageAsync(DopplerWebPushDTO message)
        {
            // TODO: consider specific implementation
            _logger.LogInformation(
                "Process message in 'Default' queue:\n\tEndpoint: {EndPoint}\n\tMessageId: {MessageId}",
                message.Subscription.EndPoint,
                message.MessageId);
            await Task.Delay(2000);
        }
    }
}
