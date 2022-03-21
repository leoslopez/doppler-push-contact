using Doppler.PushContact.Models;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IDomainService
    {
        Task UpsertAsync(Domain domain);

        Task<Domain> GetByNameAsync(string name);
    }
}
