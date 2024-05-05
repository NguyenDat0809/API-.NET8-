using Data.Models;

namespace Services.Models.Authentication.User
{
    public class LoginOTPResponse
    {
        public string? Token { get; set; }
        public bool IsTwoFactorEnable { get; set; }
        public ApplicationUser User { get; set; }
    }
}
