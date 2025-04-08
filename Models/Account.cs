using System.ComponentModel.DataAnnotations;

namespace MBBS.Dashboard.web.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        [Display(Name = "Name")]
        public string LegalName { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, ErrorMessage = "Username cannot exceed 30 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "User role is required.")]
        [StringLength(20, ErrorMessage = "User role cannot exceed 20 characters.")]
        public string UserRole { get; set; }

        public Boolean IsActive { get; set; } = true;

        
    }
}
