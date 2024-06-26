using Doppler.PushContact.QueuingService.MessageQueueBroker;
using Doppler.PushContact.WebPushSender.Logging;
using Doppler.PushContact.WebPushSender.Senders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Doppler.PushContact.WebPushSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((hostContext, loggerConfiguration) =>
                {
                    loggerConfiguration.SetupSeriLog(hostContext.Configuration, hostContext.HostingEnvironment);
                })
                .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
                {
                    // It is if you want to override the configuration in your
                    // local environment, `*.Secret.*` files will not be included in git.
                    configurationBuilder.AddJsonFile("appsettings.Secret.json", true);

                    // It is to override configuration using Docker's services.
                    // Probably this will be the way of overriding the configuration in our Swarm stack.
                    configurationBuilder.AddJsonFile("/run/secrets/appsettings.Secret.json", true);

                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;
                    services.AddMessageQueueBroker(configuration);

                    // Register IWebPushSender's
                    services.AddSingleton<IWebPushSender, DefaultWebPushSender>();

                    services.AddHostedService<SenderExecutor>();
                });
    }
}
