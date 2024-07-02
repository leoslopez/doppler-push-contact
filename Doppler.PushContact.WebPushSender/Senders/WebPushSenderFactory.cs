using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public class WebPushSenderFactory : IWebPushSenderFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public WebPushSenderFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IWebPushSender CreateSender(IOptions<WebPushSenderSettings> webPushSenderSettings)
        {
            var messageQueueSubscriber = _serviceProvider.GetRequiredService<IMessageQueueSubscriber>();
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();

            return webPushSenderSettings.Value.Type switch
            {
                WebPushSenderTypes.Default => new DefaultWebPushSender(
                    webPushSenderSettings,
                    messageQueueSubscriber,
                    loggerFactory.CreateLogger<DefaultWebPushSender>()
                ),
                _ => new DefaultWebPushSender(
                    webPushSenderSettings,
                    messageQueueSubscriber,
                    loggerFactory.CreateLogger<DefaultWebPushSender>()
                ),
            };
        }
    }

}
