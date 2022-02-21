using Doppler.PushContact.DopplerSecurity;
using Doppler.PushContact.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    public class DomainController : ControllerBase
    {
        [HttpPut]
        [Route("domains/{name}")]
        public Task<IActionResult> Upsert([FromRoute] string name, [FromBody] Domain domain)
        {
            throw new NotImplementedException();
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("domains/{name}/isPushFeatureEnabled")]
        public Task<ActionResult<bool>> GetPushFeatureStatus([FromRoute] string name)
        {
            throw new NotImplementedException();
        }
    }
}
