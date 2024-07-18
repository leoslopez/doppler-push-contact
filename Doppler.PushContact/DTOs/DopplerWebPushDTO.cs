using Doppler.PushContact.Models;
using Doppler.PushContact.Models.DTOs;

namespace Doppler.PushContact.DTOs
{
    public class DopplerWebPushDTO : WebPushDTO
    {
        public SubscriptionModel Subscription { get; set; }
        public string PushContactId { get; set; }
    }
}
