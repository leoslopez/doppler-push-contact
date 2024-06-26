using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender
{
    public class SenderExecutor : BackgroundService
    {
        public SenderExecutor()
        {
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // TODO: initialize queues
            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // TODO: define the delay time in the appsettings file
                await Task.Delay(1000);
            }
        }
    }
}
