using System.ComponentModel.DataAnnotations;

namespace FirstIterationProductRelease.Models
{
    public class ContactDetails
    {
        [Required]
        [StringLength(100, ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Subject is required")]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Message is required")]
        public string Message { get; set; }
    }
}
