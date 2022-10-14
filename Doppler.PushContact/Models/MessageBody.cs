using System.ComponentModel.DataAnnotations;

namespace Doppler.PushContact.Models
{
    public class MessageBody
    {
        [Required]
        public string Domain { get; set; }

        [Required]
        public Message Message { get; set; }
    }
}
