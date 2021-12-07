using Doppler.PushContact.Services.Messages.ExternalContracts;
using Flurl;
using Flurl.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public class MessageSender : IMessageSender
    {
        private readonly MessageSenderSettings _messageSenderSettings;
        private readonly IPushApiTokenGetter _pushApiTokenGetter;

        public MessageSender(IOptions<MessageSenderSettings> messageSenderSettings, IPushApiTokenGetter pushApiTokenGetter)
        {
            _messageSenderSettings = messageSenderSettings.Value;
            _pushApiTokenGetter = pushApiTokenGetter;
        }

        public async Task<SendMessageResult> SendAsync(string title, string body, IEnumerable<string> targetDeviceTokens, string onClickLink = null)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentException($"'{nameof(title)}' cannot be null or empty.", nameof(title));
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentException($"'{nameof(body)}' cannot be null or empty.", nameof(body));
            }

            if (targetDeviceTokens == null || !targetDeviceTokens.Any())
            {
                throw new ArgumentException($"'{nameof(targetDeviceTokens)}' cannot be null or empty.", nameof(targetDeviceTokens));
            }

            if (!string.IsNullOrEmpty(onClickLink)
                && (!Uri.TryCreate(onClickLink, UriKind.Absolute, out var onClickLinkResult) || onClickLinkResult.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException($"'{nameof(onClickLink)}' must be an absolute URL with HTTPS scheme.", nameof(onClickLink));
            }

            // TODO: use adhock token here.
            // It is recovering our client API request to be resusen to request to Push API,
            // but maybe it will not be acceptable in all scenarios.
            var pushApiToken = await _pushApiTokenGetter.GetTokenAsync();

            var responseBody = await _messageSenderSettings.PushApiUrl
                .AppendPathSegment("message")
                .WithOAuthBearerToken(pushApiToken)
                .PostJsonAsync(new
                {
                    notificationTitle = title,
                    notificationBody = body,
                    NotificationOnClickLink = onClickLink,
                    tokens = targetDeviceTokens
                })
                .ReceiveJson<SendMessageResponse>();

            return new SendMessageResult
            {
                SendMessageTargetResult = responseBody.Responses.Select(x => new SendMessageTargetResult
                {
                    TargetDeviceToken = x.DeviceToken,
                    IsSuccess = x.IsSuccess,
                    IsValidTargetDeviceToken = x.IsSuccess || _messageSenderSettings.FatalMessagingErrorCodes.All(y => y != x.Exception.MessagingErrorCode),
                    NotSuccessErrorDetails = !x.IsSuccess ? $"{nameof(x.Exception.MessagingErrorCode)} {x.Exception.MessagingErrorCode} - {nameof(x.Exception.Message)} {x.Exception.Message}" : null
                })
            };
        }
    }
}
