namespace Doppler.PushContact.Services.Messages.ExternalContracts
{
    public class SendMessageResponseException
    {
        public int MessagingErrorCode { get; set; }

        public string Message { get; set; }
    }
}
