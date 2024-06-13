using Doppler.PushContact.Controllers;
using Doppler.PushContact.DTOs;
using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Services.Queue;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Doppler.PushContact.Services
{
    public class WebPushPublisherService : IWebPushPublisherService
    {
        private readonly IPushContactService _pushContactService;
        private readonly IBackgroundQueue _backgroundQueue;
        private readonly IMessageSender _messageSender;
        private readonly ILogger<MessageController> _logger;

        public WebPushPublisherService(
            IPushContactService pushContactService,
            IBackgroundQueue backgroundQueue,
            IMessageSender messageSender,
            ILogger<MessageController> logger
        )
        {
            _pushContactService = pushContactService;
            _backgroundQueue = backgroundQueue;
            _messageSender = messageSender;
            _logger = logger;
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
                            // TODO: enqueue webpush to send with Doppler service
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
                        "An unexpected error occurred enqueing/sending webpush for domain: {domain} and messageId: {messageId}.",
                        domain,
                        messageDTO.MessageId
                    );
                }
            });
        }
    }
}
