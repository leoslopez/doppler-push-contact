using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public interface IMessageSender
    {
        Task<SendMessageResult> SendAsync(string title, string body, IEnumerable<string> targetDeviceTokens);
    }
}
