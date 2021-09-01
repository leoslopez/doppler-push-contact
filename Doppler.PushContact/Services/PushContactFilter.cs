namespace Doppler.PushContact.Services
{
    public class PushContactFilter
    {
        public string Domain { get; }

        public string Email { get; }

        public PushContactFilter(string domain, string email = null)
        {
            Domain = domain;
            Email = email;
        }
    }
}
