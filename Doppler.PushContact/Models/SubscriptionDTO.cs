namespace Doppler.PushContact.Models
{
    public class SubscriptionKeys
    {
        public string P256DH { get; set; }

        public string Auth { get; set; }
    }

    public class SubscriptionDTO
    {
        public string EndPoint { get; set; }

        public SubscriptionKeys Keys { get; set; }
    }
}
