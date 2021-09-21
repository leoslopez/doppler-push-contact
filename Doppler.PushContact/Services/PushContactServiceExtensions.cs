using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
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

            services.AddSingleton<IMongoClient>(x =>
            {
                var mongoClient = new MongoClient(mongoClientSettings);

                var database = mongoClient.GetDatabase(pushContactMongoContextSettings.DatabaseName);
                var pushContacts = database.GetCollection<BsonDocument>(pushContactMongoContextSettings.PushContactsCollectionName);

                var deviceTokenAsUniqueIndex = new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(PushContactDocumentProps.DeviceTokenPropName),
                    new CreateIndexOptions { Unique = true });
                pushContacts.Indexes.CreateOne(deviceTokenAsUniqueIndex);

                return mongoClient;
            });

            services.Configure<DeviceTokenValidatorSettings>(configuration.GetSection(nameof(DeviceTokenValidatorSettings)));

            services.AddScoped<IDeviceTokenValidator, DeviceTokenValidator>();

            services.AddScoped<IPushContactService, PushContactService>();

            return services;
        }
    }
}
