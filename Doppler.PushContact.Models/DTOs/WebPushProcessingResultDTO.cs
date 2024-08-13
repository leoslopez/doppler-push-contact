namespace Doppler.PushContact.Models.DTOs
{
    public class WebPushProcessingResultDTO
    {
        public bool SuccessfullyDelivered { get; set; }
        public bool LimitsExceeded { get; set; }
        public bool InvalidSubscription { get; set; }
        public bool FailedProcessing { get; set; }
        public bool UnknownFail { get; set; }
        public string ErrorMessage { get; set; }
    }
}
