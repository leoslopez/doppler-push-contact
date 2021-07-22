using Doppler.PushContact.Models;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IPushContactService
    {
        Task<bool> AddAsync(PushContactModel pushContactModel);
    }
}
