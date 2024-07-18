using System;

namespace Doppler.PushContact.DTOs
{
    public class WebPushEventSummarizationDTO
    {
        public Guid MessageId { get; set; }
        public int SentQuantity { get; set; }
        public int Delivered { get; set; }
        public int NotDelivered { get; set; }
    }
}
