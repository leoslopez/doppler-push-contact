using Doppler.PushContact.DTOs;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Repositories.Interfaces
{
    public interface IWebPushEventRepository
    {
        Task<WebPushEventSummarizationDTO> GetWebPushEventSummarization(Guid messageId);
    }
}
