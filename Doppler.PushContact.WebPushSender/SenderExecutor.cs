using Doppler.PushContact.WebPushSender.Senders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender
{
    public class SenderExecutor : BackgroundService
    {
        private readonly IWebPushSender _webPushSender;
        private readonly ILogger<SenderExecutor> _logger;

        public SenderExecutor(IWebPushSender webPushSender, ILogger<SenderExecutor> logger)
        {
            _webPushSender = webPushSender;
            _logger = logger;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _webPushSender.StartListeningAsync(cancellationToken);
                _logger.LogInformation($"Started listening for {_webPushSender.GetType().Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting {_webPushSender.GetType().Name}.");
                // TODO: decide what to do in case of an error when starting the sender
            }

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _webPushSender.StopListeningAsync();
                _logger.LogInformation($"Stopped listening for {_webPushSender.GetType().Name}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping {_webPushSender.GetType().Name}.");
                // TODO: decide what to do in case of an error when stopping the sender
            }

            await base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Wait indefinitely until cancellation is requested.
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
    }
}
