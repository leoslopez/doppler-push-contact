using System.Collections.Generic;

namespace Doppler.PushContact.ApiModels
{
    public class ApiPage<T>
    {
        public List<T> Items { get; }

        public int Page { get; }

        public int Per_page { get; }

        public ApiPage(List<T> items, int page, int per_page)
        {
            Items = items;
            Page = page;
            Per_page = per_page;
        }
    }
}
