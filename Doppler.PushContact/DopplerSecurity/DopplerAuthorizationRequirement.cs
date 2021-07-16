using Microsoft.AspNetCore.Authorization;

namespace Doppler.PushContact.DopplerSecurity
{
    public class DopplerAuthorizationRequirement : IAuthorizationRequirement
    {
        public bool AllowSuperUser { get; init; }
    }
}
