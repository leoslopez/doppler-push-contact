namespace Doppler.PushContact.WebPushSender.Repositories
{
    public enum WebPushEventStatus
    {
        Delivered = 0,
        Received = 1,
        Clicked = 2,
        ProcessingFailed = 3,
        DeliveryFailed = 4,
        DeliveryFailedButRetry = 5,
    }
}
