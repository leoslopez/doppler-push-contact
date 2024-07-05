using Doppler.PushContact.WebPushSender.Repositories.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories.Interfaces
{
    public interface IWebPushEventRepository
    {
        Task<bool> InsertAsync(WebPushEvent webPushEvent, CancellationToken cancellationToken = default);
    }
}
