using MongoDB.Bson;
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

        [BsonElement("message_id")]
        public Guid MessageId { get; set; }

        [BsonElement("push_contact_id")]
        public string PushContactId { get; set; }
    }
}
