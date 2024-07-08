using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Doppler.PushContact.WebPushSender.Repositories.Models
{
    public class WebPushEvent
    {
        [BsonElement(WebPushEventDocumentProps.Type_PropName)]
        public int Type { get; set; }

        [BsonElement(WebPushEventDocumentProps.Date_PropName)]
        public DateTime Date { get; set; }

        [BsonElement(WebPushEventDocumentProps.MessageId_PropName)]
        public Guid MessageId { get; set; }

        [BsonElement(WebPushEventDocumentProps.PushContactId_PropName)]
        public string PushContactId { get; set; }
    }
}
