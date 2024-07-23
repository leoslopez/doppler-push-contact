using Doppler.PushContact.Models;

namespace Doppler.PushContact.DTOs
{
    public class SubscriptionInfoDTO
    {
        public string DeviceToken { get; set; }
        public SubscriptionDTO Subscription { get; set; }
        public string PushContactId { get; set; }
    }
}
