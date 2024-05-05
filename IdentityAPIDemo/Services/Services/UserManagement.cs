using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Services.Models;
using Services.Models.Authentication.SignUp;
using System.Text;
using Services.Models.Authentication.User;
using Services.Models.Authentication.Login;
using Data.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;

namespace Services.Services
{
    public class UserManagement : IUserManagement
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserStore<ApplicationUser> _userStore;

        private readonly IConfiguration _configuration;

        public UserManagement(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IUserStore<ApplicationUser> userStore, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _configuration = configuration;
        }

        public async Task<ApiResponse<CreateUserResponse>> CreateUserWithTokenAsync(RegisterModel registerModel)
        {
            //kiểm tra user dùng email này có tồn tại hay không
            var foundedUser = await _userManager.FindByEmailAsync(email: registerModel.Email);
            if (foundedUser != null)
            {
                return new ApiResponse<CreateUserResponse> { IsSuccess = false, StatusCode = StatusCodes.Status403Forbidden, Message = $"User using {registerModel.Email} already existed!" };
            }
          
            //Tạo mới user với các thông tin cơ bản như Username, Email, SecurityStamp,..
            var user = new ApplicationUser()
            {

                Email = registerModel.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                TwoFactorEnabled = true,
                UserName = registerModel.UserName,
            };

            //thêm người dùng vào db
            var result = await _userManager.CreateAsync(user, registerModel.Password);

            if (result.Succeeded)
            {
                //tạo ra token để confirm email -> thuộc tính Email Confirmed
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //mã hóa token thành base64
                token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                return new ApiResponse<CreateUserResponse>
                {
                    IsSuccess = true,
                    StatusCode = StatusCodes.Status200OK,
                    Message = "User created successfully!",
                    Response = new CreateUserResponse { Token = token, User = user }
                };
            }
            else
            {
                return new ApiResponse<CreateUserResponse>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Role does not exist!"
                };
            }
        }

        public async Task<ApiResponse<List<string>>> AssignRoleToUserAsync(ApplicationUser user, IEnumerable<string> roles)
        {
            //tạo danh sách để lưu lại các role đã dc add cho user
            var assignedRoles = new List<string>();
            //duyệt qua từng role trong danh sách
            foreach (var role in roles)
            {
                //nếu role có tồn tại trong db thì mới thực hiện tiếp
                if (await _roleManager.RoleExistsAsync(role))
                    //nếu user chưa có role thì mới thực hiện add role cho user
                    if (!await _userManager.IsInRoleAsync(user, role))
                    {
                        await _userManager.AddToRoleAsync(user, role);
                        //role nào dc thêm thì thêm vào danh sách role đã dc thêm để trả về kết quả
                        assignedRoles.Add(role);
                    }
            }
            return new ApiResponse<List<string>>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Roles has been assigned",
                Response = assignedRoles
            };
        }

        public async Task<ApiResponse<LoginOTPResponse>> GetOTPByLogin(LoginModel loginModel)
        {
            //kiểm tra user có tồn tại
            var user = await _userManager.FindByNameAsync(loginModel.Username!);
            if (user == null)
            {
                return new ApiResponse<LoginOTPResponse>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "User not found",
                };
            }
            //kiểm tra password đã nhập
            if(!await _userManager.CheckPasswordAsync(user, loginModel.Password))
            {
                return new ApiResponse<LoginOTPResponse>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status401Unauthorized,
                    Message = "Wrong user name or password",
                };
            }
            
            //có tồn tại và đúng tk mk thì -> check tiếp bảo mật 2 lớp
            if (user.TwoFactorEnabled)
                {
                    //kiểm tra sự hiện diện của Identity.TwoFactorUserId khi user dc login và trả về mã code -> cần cho login lại để có cookie của user 
                    await _signInManager.SignOutAsync();
                    await _signInManager.PasswordSignInAsync(user, loginModel.Password, isPersistent: false, lockoutOnFailure: false);

                    var otpToken = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

                    return new ApiResponse<LoginOTPResponse>
                    {
                        IsSuccess = true,
                        StatusCode = StatusCodes.Status200OK,
                        Message = "OTP has been created",
                        Response = new LoginOTPResponse
                        {
                            Token = otpToken,
                            IsTwoFactorEnable = user.TwoFactorEnabled,
                            User = user
                        }
                    };
                }
                else
                    return new ApiResponse<LoginOTPResponse>
                    {
                        IsSuccess = false,
                        StatusCode = StatusCodes.Status403Forbidden,
                        Message = "2FA is not enabled",
                        Response = new LoginOTPResponse
                        {
                            Token = string.Empty,
                            IsTwoFactorEnable = user.TwoFactorEnabled,
                            User = user
                        }
                    };
            
        }

        public async Task<ApiResponse<LoginResponse>> GetJwtTokenAsync(ApplicationUser user)
        {
            //tạo danh sách claims
            var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, user.UserName),
                    //jti là viết tắt của JWT ID.
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };
            //thêm role vào danh sách claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            //Tạo ra access token
            var jwtToken = GetToken(claims);
            //Tạo ra refresh token
            var refreshToken = GenerateRefreshToken();
            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

            //thêm refreshtoken và expired time cho thuộc tính trong user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.Now.AddDays(refreshTokenValidityInDays);
            //gán xong thì phải update
            await _userManager.UpdateAsync(user);

            //Return token
            var jwtTokenType = new TokenType
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                ExpiryTokenDate = jwtToken.ValidTo
            };

            var rfTokenType = new TokenType
            {
                Token = refreshToken,
                ExpiryTokenDate = DateTime.Now.AddDays(refreshTokenValidityInDays)
            };
            return new ApiResponse<LoginResponse>
            {
                IsSuccess = true,
                StatusCode = StatusCodes.Status200OK,
                Message = "Tokens are created successfully",
                Response = new LoginResponse
                {
                   AccessToken = jwtTokenType,
                   RefreshToken = rfTokenType,
                }
            };

        }

        public async Task<ApiResponse<LoginResponse>> RenewAccessToken(LoginResponse token)
        {
            //lấy access token dc đưa vào
            var accessToken = token.AccessToken;
            //lấy claims
            var principal = GetClaimsPrincipal(accessToken.Token);

            //lấy refresh token dc đưa vào
            var refreshToken = token.RefreshToken;
            //lấy user
            var user = await _userManager.FindByNameAsync(principal.Identity.Name);

            if(refreshToken.Token != user.RefreshToken || refreshToken.ExpiryTokenDate <= DateTime.Now)
                return new ApiResponse<LoginResponse>
                {
                    IsSuccess = false,
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = "Token is invalid or exprired"
                };

            var tokenResponse = await GetJwtTokenAsync(user);

            return tokenResponse;   
        }

        #region Private method
        private JwtSecurityToken GetToken(List<Claim> claims)
        {
            //timespan thêm 15 tiếng
            //TimeSpan aInterval = new TimeSpan(0, 9, 0, 0);

            //tạo đối tượng khóa ký
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            //thời gian expired của token 
            _ = int.TryParse(_configuration["JWT:TokenValidityInHours"], out int tokenValidityInHours);
            //tạo đối tượng xác thức phương thức ký
            var credential = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            //tạo đối tượng giữ thông tin token
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Issuer"],
                claims,
                //thêm TimeSpan hay add bình thường đều dc hết
                //expires: DateTime.Now.AddHours(tokenValidityInHours),

                //TODO: test
                expires: DateTime.Now.AddSeconds(30),
                signingCredentials: credential
                );
            return token;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new Byte[64];
            var range = RandomNumberGenerator.Create();

            range.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal GetClaimsPrincipal(string accessToken)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])),
                ValidateLifetime = false,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);

            return principal;
        }
        #endregion
    }
}
