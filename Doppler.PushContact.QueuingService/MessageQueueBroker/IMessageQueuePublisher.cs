namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public interface IMessageQueuePublisher : IDisposable
    {
        Task PublishAsync<T>(
            T message,
            string queueName,
            CancellationToken cancellationToken
        ) where T : class;
    }
}
