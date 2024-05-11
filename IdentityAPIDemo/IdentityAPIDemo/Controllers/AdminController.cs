using IdentityAPIDemo.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace IdentityAPIDemo.Controllers
{
    [Route("api/admin")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        [HttpGet("employees")]
        [MyLogging("Caller")]
        public async Task<IActionResult> GetAllAdmins()
        {
            string[] employees = new string[] {
            "1","2","3"
            };
            Log.Information($"Employees have {employees.Count()}");
            return Ok(new { employees });
        }
    }
}
