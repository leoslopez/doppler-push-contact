using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Doppler.PushContact.Services.Messages
{
    public static class MessageSenderExtensions
    {
        public static IServiceCollection AddMessageSender(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MessageSenderSettings>(configuration.GetSection(nameof(MessageSenderSettings)));

            services.AddScoped<IMessageSender, MessageSender>();

            return services;
        }
    }
}
