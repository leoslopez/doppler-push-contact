using System.ComponentModel.DataAnnotations;

namespace Doppler.PushContact.Models
{
    public class Message
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }
    }
}
