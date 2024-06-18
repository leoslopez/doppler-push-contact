using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public static class MessageQueueBrokerExtensions
    {
        public static IServiceCollection AddMessageQueueBroker(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessageQueueBrokerSettings>(configuration.GetSection(nameof(MessageQueueBrokerSettings)));

            services.AddSingleton<IMessageQueuePublisher, RabbitMessageQueuePublisher>();

            return services;
        }
    }
}
