namespace Doppler.PushContact.Services.Messages.ExternalContracts
{
    public class SendMessageResponseDetail
    {
        public bool IsSuccess { get; set; }

        public SendMessageResponseException Exception { get; set; }

        public string DeviceToken { get; set; }
    }
}
