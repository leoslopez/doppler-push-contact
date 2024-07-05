using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Doppler.PushContact.WebPushSender.Repositories.Models
{
    public class WebPushEvent
    {
        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }
    }
}
