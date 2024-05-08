using System.ComponentModel.DataAnnotations;

namespace Services.Models.Dtos.Account
{
    public class LoginDto
    {
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
