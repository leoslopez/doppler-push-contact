using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.Repositories;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Doppler.PushContact.WebPushSender.Repositories.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public class DefaultWebPushSender : WebPushSenderBase
    {
        public DefaultWebPushSender(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger<DefaultWebPushSender> logger,
            IWebPushEventRepository weshPushEventRepository
        )
            : base(webPushSenderSettings, messageQueueSubscriber, logger, weshPushEventRepository)
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

            WebPushEvent webPushEvent = new WebPushEvent()
            {
                Date = DateTime.UtcNow,
                MessageId = message.MessageId,
                PushContactId = message.PushContactId,
            };

            if (processingResult.FailedProcessing)
            {
                webPushEvent.Type = (int)WebPushEventType.ProcessingFailed;
                // TODO: it must to retry
            }
            else if (processingResult.SuccessfullyDelivered)
            {
                webPushEvent.Type = (int)WebPushEventType.Delivered;
            }
            else if (processingResult.InvalidSubscription)
            {
                webPushEvent.Type = (int)WebPushEventType.DeliveryFailed;
                // TODO: it must to mark subscription/push-contact as "deleted"
            }
            else if (processingResult.LimitsExceeded)
            {
                webPushEvent.Type = (int)WebPushEventType.DeliveryFailedButRetry;
                // TODO: it must to retry
            }

            await _weshPushEventRepository.InsertAsync(webPushEvent);
        }
    }
}
