using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAPIDemo.Controllers
{
    [Route("api/admin")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("employees")]
        public async Task<IActionResult> GetAllAdmins()
        {
            string[] employees = new string[] {
            "1","2","3"
            };
            return Ok(new { employees });
        }
    }
}
