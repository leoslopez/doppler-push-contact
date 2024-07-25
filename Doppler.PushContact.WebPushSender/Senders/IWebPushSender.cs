using Doppler.PushContact.Models.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Senders
{
    public interface IWebPushSender
    {
        Task StartListeningAsync(CancellationToken cancellationToken);
        void StopListeningAsync();
        Task HandleMessageAsync(DopplerWebPushDTO message);
    }
}
