namespace Doppler.PushContact.Models.DTOs
{
    public class DopplerWebPushDTO : WebPushDTO
    {
        public SubscriptionDTO Subscription { get; set; }
        public string PushContactId { get; set; }
        public string ClickedEventEndpoint { get; set; }
        public string ReceivedEventEndpoint { get; set; }
    }
}
