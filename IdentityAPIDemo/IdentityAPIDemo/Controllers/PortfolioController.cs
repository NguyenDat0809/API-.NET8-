
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Repositories.Interfaces;
using System.Diagnostics.SymbolStore;
using System.Security.Claims;
using Services.ClaimsExtension;

namespace IdentityAPIDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PortfolioController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStockRepository _stockRepo;
        private readonly IPortfolioRepository _portRepo;

        public PortfolioController(UserManager<ApplicationUser> userManager, IStockRepository stockRepo, IPortfolioRepository portRepo)
        {
            _userManager = userManager;
            _stockRepo = stockRepo;
            _portRepo = portRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserPortfolio()
        {
            //User thực chất là Property do BaseController tạo ra để liên kết với ClaimsPrincial của Identity hoặc bất kì cái nào có khả năng tạo ra ClaimPrincipal
            //Khi user đã đăng nhập thì tự khắc sẽ lấy dc ClaimPrincipal
            //giải thích xong r, tiến hành lấy user dag login
            //var username = User.GetUsername();

            //Có cách khác để lấy user name, tuy nhiên cần nắm kỹ ClaimType đã dc lưu là gì
            //tôi sẽ áp dụng nó vào Porfolio Repository
            var username = User.GetUsername();

            var appUser = await _userManager.FindByNameAsync(username);

            //lấy stocks của user đó trong Portfolios
            var userPortfolios = await _portRepo.GetUserPortfolio(appUser);

            return Ok(userPortfolios);
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreatePortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var stock = await _stockRepo.GetBySybmbolAsync(symbol);

            if (stock is null)
                return BadRequest("stock not found");
            var userPortfolio = await _portRepo.GetUserPortfolio(appUser);

            if (userPortfolio.Any(x => x.Symbol.ToLower() == symbol.ToLower()))
                return BadRequest("Can not add same stock to portfolio");

            var portfolio = new Portfolio()
            {
                StockId = stock.Id,
                UserId = appUser.Id,
            };

            var createdPortfolio = await _portRepo.CreateAsync(portfolio);
            if (createdPortfolio is null)
                return BadRequest("Could not create");
            return Created();
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeletePortfolio(string symbol)
        {
            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

           var deletedPortfolio =  await _portRepo.DeleteAsync(appUser, symbol);
            if (deletedPortfolio is null)
                return BadRequest("Can not delete");

            return Ok(deletedPortfolio);
        }
    }
}
