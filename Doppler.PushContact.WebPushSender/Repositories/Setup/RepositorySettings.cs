namespace Doppler.PushContact.WebPushSender.Repositories.Setup
{
    public class RepositorySettings
    {
        public string ConnectionUrl { get; set; }

        /// <summary>
        /// Default database name to use when is not specified in ConnectionUrl
        /// </summary>
        public string DefaultDatabaseName
        {
            get
            {
                // TODO: analyze how it is better to handle the default database value
                return "push-prod";
            }
        }

        /// <summary>
        /// Secret password to use when is not specified in ConnectionUrl
        /// </summary>
        public string SecretPassword { get; set; }
    }
}
