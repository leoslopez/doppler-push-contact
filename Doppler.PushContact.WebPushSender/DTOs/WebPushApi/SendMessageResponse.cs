using System.Collections.Generic;

namespace Doppler.PushContact.WebPushSender.DTOs.WebPushApi
{
    public class SendMessageResponse
    {
        public List<SendMessageResponseDetail> Responses { get; set; }
    }
}
