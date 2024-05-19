using Services.Models.Authentication.SignUp;
using Services.Models;
using Microsoft.AspNetCore.Identity;
using Services.Models.Authentication.User;
using Services.Models.Authentication.Login;
using Data.Models;


namespace Services.Services.Interfaces
{
    public interface IUserManagement
    {
        Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterModel register);

        Task<ApiResponse<List<string>>> AssignRoleToUserAsync(ApplicationUser user, IEnumerable<string> roles);

        Task<ApiResponse<LoginOTPResponse>> GetOTPByLogin(LoginModel loginModel);

        Task<ApiResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user);

        Task<ApiResponse<LoginResponse>> RenewAccessToken(LoginResponse token);



    }
}
