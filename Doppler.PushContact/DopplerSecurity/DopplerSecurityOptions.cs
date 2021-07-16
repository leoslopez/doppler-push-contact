using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Doppler.PushContact.DopplerSecurity
{
    public class DopplerSecurityOptions
    {
        public bool SkipLifetimeValidation { get; set; }
        public IEnumerable<SecurityKey> SigningKeys { get; set; } = new SecurityKey[0];
    }
}
