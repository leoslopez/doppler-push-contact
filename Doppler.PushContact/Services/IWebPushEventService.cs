using Doppler.PushContact.DTOs;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IWebPushEventService
    {
        Task<WebPushEventSummarizationDTO> GetWebPushEventSummarizationAsync(Guid messageId);
    }
}
