using System;

namespace Doppler.PushContact.WebPushSender.DTOs.WebPushApi
{
    public class SendMessageResponseException
    {
        public int MessagingErrorCode { get; set; }
        public string Message { get; set; }
        public TimeSpan? RetryAfterSeconds { get; set; }
    }
}
