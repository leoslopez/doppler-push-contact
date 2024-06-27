namespace Doppler.PushContact.WebPushSender.DTOs
{
    public class WebPushProcessingResult
    {
        public bool SuccessfullyDelivered { get; set; }
        public bool LimitsExceeded { get; set; }
        public bool InvalidSubscription { get; set; }
        public bool FailedProcessing { get; set; }
    }
}
