namespace Doppler.PushContact.Services
{
    public class PushContactMongoContextSettings
    {
        public string MongoConnectionString { get; set; }

        public string MongoPushContactDatabaseName { get; set; }

        public string MongoPushContactCollectionName { get; set; }
    }
}
