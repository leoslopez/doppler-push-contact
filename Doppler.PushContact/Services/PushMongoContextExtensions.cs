using Doppler.PushContact.Services.Messages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Doppler.PushContact.Services
{
    public static class PushMongoContextExtensions
    {
        public static IServiceCollection AddPushMongoContext(this IServiceCollection services, IConfiguration configuration)
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

                    var domains = database.GetCollection<BsonDocument>(pushMongoContextSettings.DomainsCollectionName);

                    var domainNameAsUniqueIndex = new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys.Ascending(DomainDocumentProps.DomainNamePropName),
                    new CreateIndexOptions { Unique = true });
                    domains.Indexes.CreateOne(domainNameAsUniqueIndex);
                    return mongoClient;
                });

            return services;
        }
    }
}
