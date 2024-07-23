using Doppler.PushContact.Models;
using Doppler.PushContact.Models.DTOs;

namespace Doppler.PushContact.DTOs
{
    public class DopplerWebPushDTO : WebPushDTO
    {
        public SubscriptionDTO Subscription { get; set; }
        public string PushContactId { get; set; }
    }
}
