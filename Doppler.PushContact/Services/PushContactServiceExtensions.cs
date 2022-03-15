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
            var pushMongoContextSettingsSection = configuration.GetSection(nameof(PushMongoContextSettings));

            services.Configure<PushMongoContextSettings>(pushMongoContextSettingsSection);

            var pushMongoContextSettings = new PushMongoContextSettings();
            pushMongoContextSettingsSection.Bind(pushMongoContextSettings);

            var mongoClientSettings = MongoClientSettings.FromConnectionString(
                $"mongodb+srv://{pushMongoContextSettings.Username}:{pushMongoContextSettings.Password}@{pushMongoContextSettings.Host}");

            services.AddSingleton<IMongoClient>(x =>
            {
                var mongoClient = new MongoClient(mongoClientSettings);

                var database = mongoClient.GetDatabase(pushMongoContextSettings.DatabaseName);
                var pushContacts = database.GetCollection<BsonDocument>(pushMongoContextSettings.PushContactsCollectionName);

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
