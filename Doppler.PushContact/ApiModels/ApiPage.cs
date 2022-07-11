using System.Collections.Generic;

namespace Doppler.PushContact.ApiModels
{
    public class ApiPage<T>
    {
        public List<T> Items { get; }

        public int Page { get; }

        public ApiPage(List<T> items, int page)
        {
            Items = items;
            Page = page;
        }
    }
}
