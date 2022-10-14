using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Doppler.PushContact.DopplerSecurity;
using Doppler.PushContact.Services;
using Doppler.PushContact.Services.Messages;
using Doppler.PushContact.Models;

namespace Doppler.PushContact.Controllers
{
    [Authorize(Policies.ONLY_SUPERUSER)]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageSender _messageSender;
        private readonly IMessageRepository _messageRepository;
        private readonly IPushContactService _pushContactService;

        public MessageController(IPushContactService pushContactService, IMessageRepository messageRepository, IMessageSender messageSender)
        {
            _pushContactService = pushContactService;
            _messageRepository = messageRepository;
            _messageSender = messageSender;
        }

        [HttpPost]
        [Route("message/{messageId}")]
        public async Task<IActionResult> MessageByVisitorGuid([FromRoute] Guid messageId, [FromBody] string visitorGuid)
        {
            if (string.IsNullOrWhiteSpace(visitorGuid))
            {
                return BadRequest($"'{nameof(visitorGuid)}' cannot be null, empty or whitespace.");
            }
            if (string.IsNullOrWhiteSpace(messageId.ToString()))
            {
                return BadRequest($"'{nameof(messageId)}' cannot be null, empty or whitespace.");
            }

            var deviceTokens = await _pushContactService.GetAllDeviceTokensByVisitorGuidAsync(visitorGuid);
            var message = await _messageRepository.GetMessageDetailsByMessageIdAsync(messageId);
            var sendMessageResult = await _messageSender.SendAsync(message.Title, message.Body, deviceTokens, message.OnClickLinkPropName, message.ImageUrl);

            await _pushContactService.UpdatePushContactsAsync(messageId, sendMessageResult);

            return Ok(new MessageResult
            {
                MessageId = messageId
            });
        }

        [HttpPost]
        [Route("message")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageBody messageBody)
        {
            try
            {
                // TODO: analyze remotion of validation for the title and body, it's being doing during model binding with annotations.
                _messageSender.ValidateMessage(messageBody.Message.Title, messageBody.Message.Body, messageBody.Message.OnClickLink, messageBody.Message.ImageUrl);
            }
            catch (ArgumentException argExc)
            {
                return UnprocessableEntity(argExc.Message);
            }

            var messageId = Guid.NewGuid();

            await _messageRepository.AddAsync(
                messageId, messageBody.Domain,
                messageBody.Message.Title,
                messageBody.Message.Body,
                messageBody.Message.OnClickLink,
                0,
                0,
                0,
                messageBody.Message.ImageUrl
            );

            return Ok(new MessageResult
            {
                MessageId = messageId
            });
        }
    }
}
