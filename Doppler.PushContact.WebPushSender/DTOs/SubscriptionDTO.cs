namespace Doppler.PushContact.WebPushSender.DTOs
{
    public class SubscriptionKeysDTO
    {
        public string P256DH { get; set; }

        public string Auth { get; set; }
    }

    public class SubscriptionDTO
    {
        public string EndPoint { get; set; }

        public SubscriptionKeysDTO Keys { get; set; }
    }
}
