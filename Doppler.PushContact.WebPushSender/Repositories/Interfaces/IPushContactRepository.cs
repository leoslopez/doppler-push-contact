using System.Threading.Tasks;

namespace Doppler.PushContact.WebPushSender.Repositories.Interfaces
{
    public interface IPushContactRepository
    {
        Task<bool> MarkDeletedAsync(string pushContactId);
    }
}
