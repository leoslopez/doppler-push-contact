using System;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Queue
{
    public interface IBackgroundQueue
    {
        void QueueBackgroundQueueItem(Func<CancellationToken, Task> item);
        Task<Func<CancellationToken, Task>> DequeueItemAsync(CancellationToken cancellationToken);
    }
}
