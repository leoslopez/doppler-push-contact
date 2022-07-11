using System.Collections.Generic;

namespace Doppler.PushContact.ApiModels
{
    public class ApiPage<T>
    {
        public List<T> Items { get; set; }

        public int Position { get; set; }
    }
}
