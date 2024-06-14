using Doppler.PushContact.Controllers;
using Doppler.PushContact.DTOs;
using Doppler.PushContact.Models;
using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Services.Queue;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<WebPushPublisherService> _logger;
        private readonly IMessageQueuePublisher _messageQueuePublisher;
        private readonly Dictionary<string, List<string>> _pushEndpointMappings;

        private const string QUEUE_NAME_SUFIX = "webpush.queue";
        private const string DEFAULT_QUEUE_NAME = $"default.{QUEUE_NAME_SUFIX}";

        public WebPushPublisherService(
            IPushContactService pushContactService,
            IBackgroundQueue backgroundQueue,
            IMessageSender messageSender,
            ILogger<WebPushPublisherService> logger,
            IMessageQueuePublisher messageQueuePublisher,
            IOptions<WebPushQueueSettings> webPushQueueSettings
        )
        {
            _pushContactService = pushContactService;
            _backgroundQueue = backgroundQueue;
            _messageSender = messageSender;
            _logger = logger;
            _messageQueuePublisher = messageQueuePublisher;
            _pushEndpointMappings = webPushQueueSettings.Value.PushEndpointMappings;
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
        public string GetQueueName(string endpoint)
        {
            foreach (var mapping in _pushEndpointMappings)
            {
                foreach (var url in mapping.Value)
                {
                    if (endpoint.StartsWith(url, StringComparison.OrdinalIgnoreCase))
                    {
                        return $"{mapping.Key}.{QUEUE_NAME_SUFIX}";
                    }
                }
            }

            return DEFAULT_QUEUE_NAME;
        }
    }
}
