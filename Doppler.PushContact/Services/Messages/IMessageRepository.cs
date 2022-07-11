using Doppler.PushContact.ApiModels;
using System;
using System.Threading.Tasks;

namespace Doppler.PushContact.Services.Messages
{
    public interface IMessageRepository
    {
        Task AddAsync(Guid messageId, string domain, string title, string body, string onClickLink, int sent, int delivered, int notDelivered);

        Task<MessageDetails> GetMessageDetailsAsync(string domain, Guid messageId);

        Task<ApiPage<MessageDeliveryResult>> GetMessages(int position, int pageSize, DateTimeOffset from, DateTimeOffset to);
    }
}
