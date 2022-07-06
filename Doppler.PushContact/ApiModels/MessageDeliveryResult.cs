using System;

namespace Doppler.PushContact.ApiModels
{
    public class MessageDeliveryResult
    {
        public string Domain { get; set; }

        public int SentQuantity { get; set; }

        public int Delivered { get; set; }

        public int NotDelivered { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}
