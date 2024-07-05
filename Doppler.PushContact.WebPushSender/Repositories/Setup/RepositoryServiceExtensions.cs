using Doppler.PushContact.WebPushSender.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Doppler.PushContact.WebPushSender.Repositories.Setup
{
    public static class RepositoryServiceExtensions
    {
        public static IServiceCollection AddMongoDBRepositoryService(this IServiceCollection services, IConfiguration configuration)
        {
            var repositorySettingsSection = configuration.GetSection(nameof(RepositorySettings));
            services.Configure<RepositorySettings>(repositorySettingsSection);

            var repositorySettings = new RepositorySettings();
            repositorySettingsSection.Bind(repositorySettings);

            var mongoUrlBuilder = new MongoUrlBuilder(repositorySettings.ConnectionUrl);
            mongoUrlBuilder.DatabaseName ??= repositorySettings.DefaultDatabaseName;
            mongoUrlBuilder.Password ??= repositorySettings.SecretPassword;

            var mongoUrl = mongoUrlBuilder.ToMongoUrl();
            var mongoClient = new MongoClient(mongoUrl);
            var mongoDatabase = mongoClient.GetDatabase(mongoUrl.DatabaseName);

            services.AddSingleton<IMongoClient>(mongoClient);
            services.AddScoped(x => mongoClient.GetDatabase(mongoUrl.DatabaseName));

            services.AddScoped<IWebPushEventRepository, WebPushEventRepository>();

            return services;
        }
    }
}
