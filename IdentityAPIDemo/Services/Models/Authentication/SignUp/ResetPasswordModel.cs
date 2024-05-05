using System.ComponentModel.DataAnnotations;

namespace Services.Models.Authentication.SignUp
{
    public class ResetPasswordModel
    {
        [Required(ErrorMessage = "New password is required")]
        public string? NewPassword { get; set; } = null!;

        [Compare(nameof(NewPassword), ErrorMessage = "New Password and Confirm New Password do not match")]
        public string? ConfirmNewPassword { get; set; } = null!;
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Token { get; set; }
    }
}
