using Doppler.PushContact.ApiModels;
using Doppler.PushContact.Models;
using Doppler.PushContact.Services.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services
{
    public interface IPushContactService
    {
        Task AddAsync(PushContactModel pushContactModel);

        Task UpdateEmailAsync(string deviceToken, string email);

        Task<IEnumerable<PushContactModel>> GetAsync(PushContactFilter pushContactFilter);

        Task<long> DeleteByDeviceTokenAsync(IEnumerable<string> deviceTokens);

        Task AddHistoryEventsAsync(IEnumerable<PushContactHistoryEvent> pushContactHistoryEvents);

        Task UpdatePushContactsAsync(Guid messageId, SendMessageResult sendMessageResult);

        Task<IEnumerable<string>> GetAllDeviceTokensByDomainAsync(string domain);

        Task<IEnumerable<string>> GetAllDeviceTokensByVisitorGuidAsync(string visitorGuid);

        Task<ApiPage<DomainInfo>> GetDomains(int page, int per_page);

        Task<MessageDeliveryResult> GetHistoryEventResultByMessageIdAsync(string domain, Guid messageId);

        Task UpdatePushContactVisitorGuid(string deviceToken, string visitorGuid);

        Task<ApiPage<string>> GetAllVisitorGuidByDomain(string domain, int page, int per_page);
    }
}
