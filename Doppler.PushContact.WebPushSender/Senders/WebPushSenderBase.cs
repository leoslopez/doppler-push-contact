using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Doppler.PushContact.WebPushSender.DTOs.WebPushApi;
using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public abstract class WebPushSenderBase : IWebPushSender
    {
        private const string PUSHAPI_SENDWEBPUSH_PATH = "webpush";

        private readonly IMessageQueueSubscriber _messageQueueSubscriber;
        protected readonly ILogger _logger;
        protected readonly string _queueName;
        private IDisposable _queueSubscription;
        private readonly string _pushApiUrl;
        protected readonly IWebPushEventRepository _weshPushEventRepository;

        protected WebPushSenderBase(
            IOptions<WebPushSenderSettings> webPushSenderSettings,
            IMessageQueueSubscriber messageQueueSubscriber,
            ILogger logger,
            IWebPushEventRepository weshPushEventRepository
        )
        {
            _messageQueueSubscriber = messageQueueSubscriber;
            _logger = logger;
            _queueName = webPushSenderSettings.Value.QueueName;
            _pushApiUrl = webPushSenderSettings.Value.PushApiUrl;
            _weshPushEventRepository = weshPushEventRepository;
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

        protected virtual async Task<WebPushProcessingResult> SendWebPush(DopplerWebPushDTO message)
        {
            SendMessageResponse sendMessageResponse = null;
            try
            {
                sendMessageResponse = await _pushApiUrl
                .AppendPathSegment(PUSHAPI_SENDWEBPUSH_PATH)
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

                return new WebPushProcessingResult()
                {
                    FailedProcessing = true,
                };
            }

            return ProcessWebPushResponse(sendMessageResponse);
        }

        private WebPushProcessingResult ProcessWebPushResponse(SendMessageResponse sendMessageResponse)
        {
            if (sendMessageResponse == null)
            {
                return new WebPushProcessingResult { FailedProcessing = false };
            }

            WebPushProcessingResult processingResult = new WebPushProcessingResult();

            // it has just one response
            var response = sendMessageResponse.Responses?.FirstOrDefault();
            processingResult.SuccessfullyDelivered = response != null && response.IsSuccess == true;

            if (response != null && response.IsSuccess == false && response.Exception != null)
            {
                switch (response.Exception.MessagingErrorCode)
                {
                    case 429:
                        // TODO: log information to be analyzed and take proper actions
                        _logger.LogWarning
                        (
                            "(Error {WebPushResponseStatusCode}) Too many requests:\n\tSubscription: {Subscription}\n\tException: {WebPushResponseException}",
                            response.Exception.MessagingErrorCode,
                            JsonConvert.SerializeObject(response.Subscription),
                            JsonConvert.SerializeObject(response.Exception)
                        );

                        processingResult.LimitsExceeded = true;
                        break;

                    case (int)HttpStatusCode.NotFound:
                    case (int)HttpStatusCode.Gone:
                        _logger.LogDebug
                        (
                            "(Error {WebPushResponseStatusCode}):\n\tSubscription: {Subscription}\n\tException: {WebPushResponseException}",
                            response.Exception.MessagingErrorCode,
                            JsonConvert.SerializeObject(response.Subscription),
                            JsonConvert.SerializeObject(response.Exception)
                        );

                        processingResult.InvalidSubscription = true;
                        break;
                }
            }

            return processingResult;
        }
    }
}
