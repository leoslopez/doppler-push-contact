using Doppler.PushContact.Services.Queue;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public class QueueBackgroundService : BackgroundService
    {
        private readonly IBackgroundQueue _backgroundQueue;

        public QueueBackgroundService(IBackgroundQueue backgroundQueue)
        {
            _backgroundQueue = backgroundQueue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var item = await _backgroundQueue.DequeueItemAsync(stoppingToken);

                await item(stoppingToken);
            }
        }
    }
}
