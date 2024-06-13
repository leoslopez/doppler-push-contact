using Doppler.PushContact.Controllers;
using Doppler.PushContact.DTOs;
using Doppler.PushContact.Models;
using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Services.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public class WebPushPublisherService : IWebPushPublisherService
    {
        private readonly IPushContactService _pushContactService;
        private readonly IBackgroundQueue _backgroundQueue;
        private readonly IMessageSender _messageSender;
        private readonly ILogger<MessageController> _logger;
        private readonly IMessageQueuePublisher _messageQueuePublisher;

        public WebPushPublisherService(
            IPushContactService pushContactService,
            IBackgroundQueue backgroundQueue,
            IMessageSender messageSender,
            ILogger<MessageController> logger,
            IMessageQueuePublisher messageQueuePublisher
        )
        {
            _pushContactService = pushContactService;
            _backgroundQueue = backgroundQueue;
            _messageSender = messageSender;
            _logger = logger;
            _messageQueuePublisher = messageQueuePublisher;
        }

        public void ProcessWebPush(string domain, WebPushDTO messageDTO, string authenticationApiToken = null)
        {
            _backgroundQueue.QueueBackgroundQueueItem(async (cancellationToken) =>
            {
                try
                {
                    var deviceTokens = new List<string>();
                    var subscriptionsInfo = await _pushContactService.GetAllSubscriptionInfoByDomainAsync(domain);
                    foreach (var subscription in subscriptionsInfo)
                    {
                        if (subscription.Subscription != null &&
                            subscription.Subscription.Keys != null &&
                            !string.IsNullOrEmpty(subscription.Subscription.EndPoint) &&
                            !string.IsNullOrEmpty(subscription.Subscription.Keys.Auth) &&
                            !string.IsNullOrEmpty(subscription.Subscription.Keys.P256DH)
                        )
                        {
                            await EnqueueWebPushAsync(messageDTO, subscription.Subscription, cancellationToken);
                        }
                        else if (!string.IsNullOrEmpty(subscription.DeviceToken))
                        {
                            deviceTokens.Add(subscription.DeviceToken);
                        }
                    }

                    await _messageSender.SendFirebaseWebPushAsync(messageDTO, deviceTokens, authenticationApiToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "An unexpected error occurred processing webpush for domain: {domain} and messageId: {messageId}.",
                        domain,
                        messageDTO.MessageId
                    );
                }
            });
        }

        private async Task EnqueueWebPushAsync(WebPushDTO messageDTO, SubscriptionModel subscription, CancellationToken cancellationToken)
        {
            var webPushMessage = new DopplerWebPushDTO()
            {
                Title = messageDTO.Title,
                Body = messageDTO.Body,
                OnClickLink = messageDTO.OnClickLink,
                ImageUrl = messageDTO.ImageUrl,
                Subscription = subscription,
                MessageId = messageDTO.MessageId,
            };

            string queueName = GetQueueName(subscription.EndPoint);

            try
            {
                await _messageQueuePublisher.PublishAsync(webPushMessage, queueName, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An unexpected error occurred enqueuing webpush for messageId: {messageId} and subscription: {subscription}.",
                    messageDTO.MessageId,
                    JsonSerializer.Serialize(subscription, new JsonSerializerOptions { WriteIndented = true })
                );
            }
        }

        // TODO: obtains queue names and endpoints for each service from config
        private string GetQueueName(string endpoint)
        {
            if (endpoint.StartsWith("https://fcm.googleapis.com", StringComparison.OrdinalIgnoreCase))
            {
                return "google.notification.queue";
            }

            if (endpoint.StartsWith("https://updates.push.services.mozilla.com", StringComparison.OrdinalIgnoreCase))
            {
                return "mozilla.notification.queue";
            }

            if (endpoint.StartsWith("https://wns.windows.com", StringComparison.OrdinalIgnoreCase))
            {
                return "microsoft.notification.queue";
            }

            if (endpoint.StartsWith("https://api.push.apple.com", StringComparison.OrdinalIgnoreCase))
            {
                return "apple.notification.queue";
            }

            return "default.notification.queue";
        }
    }
}
