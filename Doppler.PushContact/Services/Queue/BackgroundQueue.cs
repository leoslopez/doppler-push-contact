using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Queue
{
    public class BackgroundQueue : IBackgroundQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _queue;
        private SemaphoreSlim _semaphore;

        public BackgroundQueue()
        {
            _queue = new ConcurrentQueue<Func<CancellationToken, Task>>();
            _semaphore = new SemaphoreSlim(0);
        }
        public async Task<Func<CancellationToken, Task>> DequeueItemAsync(CancellationToken cancellationToken)
        {
            await _semaphore.WaitAsync(cancellationToken);

            _queue.TryDequeue(out var item);

            return item;
        }

        public void QueueBackgroundQueueItem(Func<CancellationToken, Task> item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item is null");
            }

            _queue.Enqueue(item);

            _semaphore.Release();
        }
    }
}
