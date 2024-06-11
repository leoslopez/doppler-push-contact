using EasyNetQ.ConnectionString;
using EasyNetQ;
using Microsoft.Extensions.Options;
using EasyNetQ.Topology;
using EasyNetQ.DI;

namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public class RabbitMessageQueuePublisher : IMessageQueuePublisher
    {
        private readonly IBus _bus;

        public RabbitMessageQueuePublisher(IOptions<MessageQueueBrokerSettings> options)
        {
            var connectionConfiguration = new ConnectionStringParser().Parse(options.Value.ConnectionString);
            if (!string.IsNullOrWhiteSpace(options.Value.Password))
            {
                connectionConfiguration.Password = options.Value.Password;
            }

            _bus = RabbitHutch.CreateBus(connectionConfiguration, registerServices =>
            {
                registerServices.Register<ISerializer, EasyNetQ.Serialization.SystemTextJson.SystemTextJsonSerializer>();
            });
        }

        public async Task PublishAsync<T>(T message, string queueName, CancellationToken cancellationToken) where T : class
        {
            await _bus.Advanced.QueueDeclareAsync(queueName, cancellationToken);
            await _bus.Advanced.PublishAsync(Exchange.Default, queueName, mandatory: true, new Message<T>(message), cancellationToken);
        }

        #region IDisposable

        private bool isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed) return;

            if (disposing)
            {
                _bus.Dispose();
            }

            isDisposed = true;
        }

        #endregion
    }
}
