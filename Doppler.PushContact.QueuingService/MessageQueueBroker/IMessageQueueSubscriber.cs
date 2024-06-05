namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public interface IMessageQueueSubscriber : IDisposable
    {
        Task<IDisposable> SubscribeAsync<T>(
            Func<T, Task> handler,
            string? queueName = null,
            CancellationToken cancellationToken = default
            ) where T : class;
    }
}
