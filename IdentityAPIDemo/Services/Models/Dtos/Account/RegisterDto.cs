using System.ComponentModel.DataAnnotations;

namespace Services.Models.Dtos.Account
{
    public class RegisterDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Compare("Password", ErrorMessage = "Password and Confirmation Password must match.")]
        public string? ConfirmPassword { get; set; }
    }
}
