using System.Collections.Generic;

namespace Doppler.PushContact.Services.Messages.ExternalContracts
{
    public class SendMessageResponse
    {
        public List<SendMessageResponseDetail> Responses { get; set; }
    }
}
