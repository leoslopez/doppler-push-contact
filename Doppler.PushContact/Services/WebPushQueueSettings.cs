using System.Collections.Generic;

namespace Doppler.PushContact.Services
{
    public class WebPushQueueSettings
    {
        public Dictionary<string, List<string>> PushEndpointMappings { get; set; }
    }
}
