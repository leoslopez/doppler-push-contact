using System.ComponentModel.DataAnnotations;

namespace Doppler.PushContact.Models
{
    public class MessageBody
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public string OnClickLink { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public string Domain { get; set; }
    }
}
