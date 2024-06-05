using EasyNetQ;
using EasyNetQ.ConnectionString;
using EasyNetQ.DI;
using Microsoft.Extensions.Options;

namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public class RabbitMessageQueueSubscriber : IMessageQueueSubscriber
    {
        private readonly IBus _bus;

        public RabbitMessageQueueSubscriber(IOptions<MessageQueueBrokerSettings> options)
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

        /// <inheritdoc/>
        public async Task<IDisposable> SubscribeAsync<T>(Func<T, Task> action, string? queueName = null, CancellationToken cancellationToken = default) where T : class
        {
            var queue = await _bus.Advanced.QueueDeclareAsync(queueName, cancellationToken);
            return _bus.Advanced.Consume(queue, async (body, properties, info) =>
            {
                using var stream = new MemoryStream(body.ToArray());
                using var reader = new StreamReader(stream);
                var jsonMessage = await reader.ReadToEndAsync();
                var message = System.Text.Json.JsonSerializer.Deserialize<T>(jsonMessage);
                if (message != null)
                {
                    await action.Invoke(message);
                }
            });
        }

        #region IDisposable

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                _bus.Dispose();
            }

            _isDisposed = true;
        }

        #endregion
    }
}
