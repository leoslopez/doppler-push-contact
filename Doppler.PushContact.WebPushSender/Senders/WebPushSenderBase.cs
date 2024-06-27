using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.DTOs.WebPushApi;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public abstract class WebPushSenderBase : IWebPushSender
    {
        private readonly IMessageQueueSubscriber _messageQueueSubscriber;
        protected readonly ILogger _logger;
        protected readonly string _queueName;
        private IDisposable _queueSubscription;

        protected WebPushSenderBase(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger logger
        )
        {
            _messageQueueSubscriber = messageQueueSubscriber;
            _logger = logger;
            _queueName = webPushSenderSettings.Value.QueueName;
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            _queueSubscription = await _messageQueueSubscriber.SubscribeAsync<DopplerWebPushDTO>(
                HandleMessageAsync,
                _queueName,
                cancellationToken
            );
        }

        public void StopListeningAsync()
        {
            if (_queueSubscription != null)
            {
                _queueSubscription.Dispose();
            }
        }

        public abstract Task HandleMessageAsync(DopplerWebPushDTO message);

        protected async Task SendWebPush(DopplerWebPushDTO message)
        {
            // TODO: add value in appsettings file
            var pushApiUrl = "https://apisint.fromdoppler.net/doppler-push";

            SendMessageResponse sendMessageResponse = null;
            try
            {
                sendMessageResponse = await pushApiUrl
                .AppendPathSegment("webpush")
                // TODO: analyze options to handle (or remove) push api token
                //.WithOAuthBearerToken(pushApiToken)
                .PostJsonAsync(new
                {
                    subscriptions = new[]
                    {
                        new
                        {
                            endpoint = message.Subscription.EndPoint,
                            p256DH = message.Subscription.Keys.P256DH,
                            auth = message.Subscription.Keys.Auth,
                        }
                    },
                    notificationTitle = message.Title,
                    notificationBody = message.Body,
                    notificationOnClickLink = message.OnClickLink,
                    imageUrl = message.ImageUrl,
                })
                .ReceiveJson<SendMessageResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error happened sending web push notification: {ex}");
            }
        }
    }
}
