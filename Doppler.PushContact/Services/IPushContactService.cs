using Doppler.PushContact.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IPushContactService
    {
        Task<bool> AddAsync(PushContactModel pushContactModel);

        Task<IEnumerable<PushContactModel>> GetAsync(PushContactFilter pushContactFilter);

        Task<long> DeleteByDeviceTokenAsync(IEnumerable<string> deviceTokens);
    }
}
