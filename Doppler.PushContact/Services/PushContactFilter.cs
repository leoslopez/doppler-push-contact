namespace Doppler.PushContact.Services
{
    public class PushContactFilter
    {
        public string Domain { get; }

        public PushContactFilter(string domain)
        {
            Domain = domain;
        }
    }
}
