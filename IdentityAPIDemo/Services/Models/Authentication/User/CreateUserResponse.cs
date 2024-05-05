using Data.Models;

namespace Services.Models.Authentication.User
{
    public class CreateUserResponse
    {
        public string? Token { get; set; }
        public ApplicationUser User { get; set; }
    }
}
