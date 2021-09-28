using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public class PushApiTokenGetter : IPushApiTokenGetter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PushApiTokenGetter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _httpContextAccessor.HttpContext.GetTokenAsync("Bearer", "access_token");
        }
    }
}
