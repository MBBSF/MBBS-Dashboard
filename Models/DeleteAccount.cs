using System.ComponentModel.DataAnnotations;

namespace FirstIterationProductRelease.Models
{
    public class DeleteAccount
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Please enter your current password")]
        public string CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Please confirm your password")]
        [Compare("CurrentPassword", ErrorMessage =
            "Your password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}