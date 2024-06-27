namespace Doppler.PushContact.WebPushSender.DTOs.WebPushApi
{
    public class SendMessageResponseDetail
    {
        public bool IsSuccess { get; set; }
        public SendMessageResponseException Exception { get; set; }
        public SubscriptionResponse Subscription { get; set; }
    }
}
