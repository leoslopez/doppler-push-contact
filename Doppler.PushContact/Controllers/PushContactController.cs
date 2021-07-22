using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Doppler.PushContact.Models;
using Doppler.PushContact.DopplerSecurity;
using System;
using Doppler.PushContact.Services;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    [Route("[controller]")]
    public class PushContactController : ControllerBase
    {
        private readonly IPushContactService _pushContactService;

        public PushContactController(IPushContactService pushContactService)
        {
            _pushContactService = pushContactService;
        }

        [HttpPost("/push-contact/add")]
        public async Task<IActionResult> Add([FromBody] PushContactModel pushContactModel)
        {
            var added = await _pushContactService.AddAsync(pushContactModel);

            if (!added)
            {
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
