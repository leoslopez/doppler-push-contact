using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Doppler.PushContact.Services
{
    public static class PushContactServiceExtensions
    {
        public static IServiceCollection AddPushContactService(this IServiceCollection services, IConfiguration configuration)
        {
            var pushContactMongoContextSettingsSection = configuration.GetSection(nameof(PushContactMongoContextSettings));

            services.Configure<PushContactMongoContextSettings>(pushContactMongoContextSettingsSection);

            var pushContactMongoContextSettings = new PushContactMongoContextSettings();
            pushContactMongoContextSettingsSection.Bind(pushContactMongoContextSettings);

            var mongoClientSettings = MongoClientSettings.FromConnectionString(
                $"mongodb+srv://{pushContactMongoContextSettings.Username}:{pushContactMongoContextSettings.Password}@{pushContactMongoContextSettings.Host}");

            services.AddSingleton<IMongoClient>(x => new MongoClient(mongoClientSettings));

            services.AddScoped<IDeviceTokenValidator, DeviceTokenValidator>();

            services.AddScoped<IPushContactService, PushContactService>();

            return services;
        }
    }
}
