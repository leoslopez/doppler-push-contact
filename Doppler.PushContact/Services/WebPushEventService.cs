using Doppler.PushContact.DTOs;
using Doppler.PushContact.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public class WebPushEventService : IWebPushEventService
    {
        private readonly IWebPushEventRepository _webPushEventRepository;
        private readonly ILogger<WebPushEventService> _logger;

        public WebPushEventService(
            IWebPushEventRepository webPushEventRepository,
            ILogger<WebPushEventService> logger
        )
        {
            _webPushEventRepository = webPushEventRepository;
            _logger = logger;
        }

        public async Task<WebPushEventSummarizationDTO> GetWebPushEventSummarizationAsync(Guid messageId)
        {
            try
            {
                return await _webPushEventRepository.GetWebPushEventSummarization(messageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error summarizing 'WebPushEvents' with {nameof(messageId)} {messageId}");
                return new WebPushEventSummarizationDTO()
                {
                    MessageId = messageId,
                    SentQuantity = 0,
                    Delivered = 0,
                    NotDelivered = 0,
                };
            }
        }
    }
}
