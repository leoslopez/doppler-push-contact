namespace Doppler.PushContact.WebPushSender.DTOs
{
    public class DopplerWebPushDTO : WebPushDTO
    {
        public SubscriptionDTO Subscription { get; set; }
        public string PushContactId { get; set; }
    }
}
