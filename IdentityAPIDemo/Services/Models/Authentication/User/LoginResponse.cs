using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Models.Authentication.User
{
    public class LoginResponse
    {
        //jwt token
        public TokenType AccessToken { get; set; }
        //refresh token
        public TokenType RefreshToken { get; set; }
    }
}
