using System;

namespace Doppler.PushContact.Services
{
    public class PushContactFilter
    {
        public string Domain { get; }

        public string Email { get; }

        public DateTime? ModifiedFrom { get; }

        public DateTime? ModifiedTo { get; }

        public PushContactFilter(string domain, string email = null, DateTime? modifiedFrom = null, DateTime? modifiedTo = null)
        {
            Domain = domain;
            Email = email;
            ModifiedFrom = modifiedFrom;
            ModifiedTo = modifiedTo;
        }
    }
}
