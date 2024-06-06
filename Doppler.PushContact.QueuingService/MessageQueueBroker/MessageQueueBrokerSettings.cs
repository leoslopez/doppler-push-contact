namespace Doppler.PushContact.QueuingService.MessageQueueBroker
{
    public class MessageQueueBrokerSettings
    {
        public required string ConnectionString { get; set; }

        /// <summary>
        /// Password for connect to MessageQueueSubscriber.
        /// If ConnectionString has defined password parameter, will be replaced with this value if it is not empty.
        /// </summary>
        public string? Password { get; set; }
    }
}
