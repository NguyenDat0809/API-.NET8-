using Data.Models;
using IdentityAPIDemo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services.Models;
using Services.Models.Authentication.Login;
using Services.Models.Authentication.SignUp;
using Services.Models.Authentication.User;
using Services.Services.Interface;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace IdentityAPIDemo.Controllers
{
    [Route("api/authentication")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IUserStore<ApplicationUser> _userStore;

        private readonly IEmailService _emailService;

        private readonly IConfiguration _configuration;

        private readonly IUserManagement _user;


        public AuthenticationController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager, IUserStore<ApplicationUser> userStore, IEmailService emailService, IConfiguration configuration, IUserManagement user)
        {
            _roleManager = roleManager;
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailService = emailService;
            _configuration = configuration;

            _user = user;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginUser)
        {
            if (ModelState.IsValid)
            {
                var loginOTPResponse = await _user.GetOTPByLogin(loginUser);

                if (loginOTPResponse.Response != null)
                {
                    var user = loginOTPResponse.Response.User;

                    //tiến hành đưa mã OTP qua email
                    //nếu có xác thực 2 yếu tố thì yêu cầu người dùng lấy code qua email
                    if (user.TwoFactorEnabled)
                    {
                        var token = loginOTPResponse.Response.Token;
                        //TODO: test
                        //var message = new Message(new[] { "datnx080903@gmail.com" }, "OTP Confirmation", otpToken);

                        var message = new Message(new[] { user.Email }, "OTP Confirmation", token);

                        _emailService.SendEmail(message);

                        return StatusCode(StatusCodes.Status200OK, new Response
                        {
                            IsSuccess = loginOTPResponse.IsSuccess,
                            Status = "Success",
                            Message = $"We have sent an OTP to your Email {user.Email}"
                        });
                    }
                }
                return Unauthorized();
            }
            return BadRequest(ModelState);
        }

        [HttpPost("login-2FA")]
        public async Task<IActionResult> LoginWithOTP(string otp, string username)
        {
            //đăng nhập bằng 2FA 
            //Lưu ý: đối với TwoFactorSignIn thì hàm này cũng sẽ kiểm tra EmailConfirmed, nếu chưa xác thực email thì có nhận được OTP thì SẼ KO CHO PHÉP ĐĂNG NHẬP BẰNG 2 YẾU TỐ qua email
            var signInResult = await _signInManager.TwoFactorSignInAsync("Email", otp, isPersistent: false, rememberClient: false);

            //kiểm tra kết qua từ đăng nhập 2FA
            if (signInResult.Succeeded)
            {
                //lấy user từ username
                var user = await _userManager.FindByNameAsync(username);

                //nếu user tồn tại, tạo danh sách claims
                if (user != null)
                {
                    //Tạo ra JWT token response
                    var jwtTokenResponse = await _user.GetJwtTokenAsync(user);

                    var jwtToken = jwtTokenResponse.Response.AccessToken.Token;
                    var expiryJwtToken = jwtTokenResponse.Response.AccessToken.ExpiryTokenDate;
                    //Return token
                    return StatusCode(StatusCodes.Status200OK, new TokenType
                    {
                        Token = jwtToken,
                        ExpiryTokenDate = expiryJwtToken,
                    });

                }
                return StatusCode(StatusCodes.Status400BadRequest, new Response
                {
                    IsSuccess = false,
                    Status = "Error",
                    Message = "User does not exist !"
                });
            }
            return StatusCode(StatusCodes.Status404NotFound, new Response
            {
                IsSuccess = false,
                Status = "Error",
                Message = "Invalid OTP code !"
            });
        }


        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterModel registerUser)
        {
            if (ModelState.IsValid)
            {
                var createResponse = await _user.CreateUserWithTokenAsync(registerUser);

                if (createResponse.IsSuccess)
                {
                    //thêm role cho user
                    await _user.AssignRoleToUserAsync(createResponse.Response.User, registerUser.Roles);

                    //tạo đường dẫn đến api email confirm
                    var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { createResponse.Response.Token, email = registerUser.Email }, Request.Scheme);

                    //TODO: test
                    var message = new Message(new[] { "datnx080903@gmail.com" }, "Confirmation email link", confirmationLink);


                    //tạo đối tượng Message chứa nội dung tin nhắn
                    //var message = new Message(new[] { registerUser.Email }, "Confirmation email link", confirmationLink);
                    //gửi tin nhắn 
                    _emailService.SendEmail(message);

                    return StatusCode(StatusCodes.Status200OK, new Response
                    {
                        Status = "Success",
                        Message = $"User created & Email send to {registerUser.Email} successfully!",
                        IsSuccess = true
                    });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new Response
                {
                    Status = "Error",
                    Message = createResponse.Message,
                    IsSuccess = false
                });

            }
            else
                return StatusCode(StatusCodes.Status400BadRequest, ModelState);


        }

        //[HttpGet("send-mail")]
        //public async Task<IActionResult> TestEmail()
        //{

        //    var message = new Message(new []{ "vanlttss171149@fpt.edu.vn" }, "Test mail function", "Test ");

        //    _emailService.SendEmail(message);
        //    return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Success", Message = "Email sent successfully" });
        //}


        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            //giải mã
            /*
                token.Replace('_', '/').Replace('-', '+'): Chuỗi mã hóa Base64 URL sử dụng các ký tự ‘_’ và ‘-’ thay cho ‘/’ và ‘+’ để làm cho chuỗi có thể an toàn khi sử dụng trong URL. Đoạn mã này chuyển đổi lại các ký tự này về dạng chuẩn của Base64.
             */
            token = token.Replace('_', '/').Replace('-', '+');
            switch (token.Length % 4)
            {
                /*
                 case 2: Nếu chuỗi mã hóa sau khi đã thay thế ký tự có độ dài chia cho 4 dư 2, nghĩa là nó thiếu 2 ký tự để trở thành bội số của 4. Trong trường hợp này, chúng ta thêm vào “==” để đạt đủ độ dài cần thiết.
                 */
                case 2: token += "=="; break;

                /*
                 case 3: Nếu chuỗi mã hóa sau khi đã thay thế ký tự có độ dài chia cho 4 dư 3, nghĩa là nó thiếu 1 ký tự để trở thành bội số của 4. Trong trường hợp này, chúng ta thêm vào “=” để đạt đủ độ dài cần thiết.
                 */
                case 3: token += "="; break;
                    //khi mã hóa '=' sẽ bị bỏ qua
            }
            //chuyển đổi token đã mã hóa thành mảng byte
            var decoded = Convert.FromBase64String(token);
            //chuyển đổi mảng byte đó thành một chuỗi văn bản.Bước này sử dụng bảng mã mặc định của hệ thống để chuyển đổi từ dữ liệu nhị phân sang văn bản có thể đọc được.
            var decodedToken = Encoding.Default.GetString(decoded);
            token = decodedToken.Trim();

            //Tìm kiếm user từ email đưa vào từ querystring
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                //nếu confirm thành công thì thuộc tính EmailConfirmed của User thành true
                var confirmResult = await _userManager.ConfirmEmailAsync(user, token);
                if (confirmResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, new Response { Status = "Success", Message = "Email verified successfully!" });
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = "User does not exist" });
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgetPassword([Required] string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                var resetPasswordLink = Url.Action(nameof(ResetPassword), "Authentication", new { token = resetToken, email = user.Email }, Request.Scheme);

                var message = new Message(new[] { user.Email }, "Reset Password Confirmaton Link", resetPasswordLink);

                _emailService.SendEmail(message);

                return StatusCode(StatusCodes.Status200OK, new Response
                {
                    Status = "Success",
                    Message = $"A reset password redirtion has been sent to your Email {user.Email}.Please open your email to check"
                });
            }
            return StatusCode(StatusCodes.Status404NotFound, new Response
            {
                Status = "Error",
                Message = "User not found"
            });
        }

        [HttpGet("reset-password")]
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            var resetPassModel = new ResetPasswordModel()
            {
                Email = email,
                Token = token
            };

            return Ok(new { resetPassModel });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPassModel)
        {
            var user = await _userManager.FindByEmailAsync(resetPassModel.Email!);

            if (user != null)
            {
                var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPassModel.Token, resetPassModel.NewPassword);

                if (!resetPassResult.Succeeded)
                {
                    foreach (var error in resetPassResult.Errors)
                    {
                        ModelState.AddModelError(key: error.Code, errorMessage: error.Description);
                    }
                    return StatusCode(StatusCodes.Status500InternalServerError, ModelState);
                }
                return StatusCode(StatusCodes.Status200OK, new Response
                {
                    Status = "Success",
                    Message = $"Password has changed"
                });
            }
            return StatusCode(StatusCodes.Status404NotFound, new Response
            {
                Status = "Error",
                Message = $"User not found"
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshAccessToken(LoginResponse accessToken)
        {
            var jwtResponse = await _user.RenewAccessToken(accessToken);
            if (jwtResponse.IsSuccess)
            {
                return Ok(jwtResponse);
            }

            return StatusCode(StatusCodes.Status404NotFound, new Response
            {
                Status = "Error",
                Message = "Invalid Code",
                IsSuccess = false
            });
        }

        #region Private functions
        //private ApplicationUser CreateUser()
        //{
        //    return Activator.CreateInstance<ApplicationUser>();
        //}



        #endregion




    }
}
