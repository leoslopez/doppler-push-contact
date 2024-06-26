using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.DTOs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public abstract class WebPushSenderBase : IWebPushSender
    {
        private readonly IMessageQueueSubscriber _messageQueueSubscriber;
        protected readonly ILogger _logger;
        private readonly string _queueName;
        private IDisposable _queueSubscription;

        protected WebPushSenderBase(string queueName, IMessageQueueSubscriber messageQueueConsumer, ILogger logger)
        {
            _messageQueueSubscriber = messageQueueConsumer;
            _logger = logger;
            _queueName = queueName;
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
    }
}
