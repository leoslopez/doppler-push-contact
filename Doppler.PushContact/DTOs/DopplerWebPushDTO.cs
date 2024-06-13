using Doppler.PushContact.Models;

namespace Doppler.PushContact.DTOs
{
    public class DopplerWebPushDTO : WebPushDTO
    {
        public SubscriptionModel Subscription { get; set; }
    }
}
