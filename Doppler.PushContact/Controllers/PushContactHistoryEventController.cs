using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Doppler.PushContact.Models;
using Doppler.PushContact.DopplerSecurity;
using Doppler.PushContact.Services;
using System.Collections.Generic;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    public class PushContactHistoryEventController : ControllerBase
    {
        private readonly IPushContactService _pushContactService;

        public PushContactHistoryEventController(IPushContactService pushContactService)
        {
            _pushContactService = pushContactService;
        }

        [HttpPost]
        [Route("history-events/_bulk")]
        public async Task<IActionResult> BulkAdd([FromBody] IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents)
        {
            await _pushContactService.AddHistoryEventsAsync(pushContactHistoryEvents);

            return Ok();
        }
    }
}
