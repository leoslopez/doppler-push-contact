namespace Doppler.PushContact.WebPushSender.DTOs.WebPushApi
{
    public class SubscriptionResponse
    {
        public string Endpoint { get; set; }
        public string P256DH { get; set; }
        public string Auth { get; set; }
    }
}
