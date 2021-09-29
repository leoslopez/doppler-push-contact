using System.Collections.Generic;

namespace Doppler.PushContact.Services.Messages
{
    public class SendMessageResult
    {
        public IEnumerable<SendMessageTargetResult> SendMessageTargetResult { get; set; }
    }
}
