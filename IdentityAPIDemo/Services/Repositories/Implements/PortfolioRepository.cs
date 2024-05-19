
using Data.Models;
using Microsoft.EntityFrameworkCore;
using Services.Repositories.Interfaces;

namespace FinShark.Repositories
{
    public class Portfoliorepository : IPortfolioRepository
    {
        private readonly ApplicationDbContext _db;

        public Portfoliorepository(ApplicationDbContext db)
        {
            _db =db;
        }

        public async Task<Portfolio> CreateAsync(Portfolio portfolio)
        {
            await _db.Portfolios.AddAsync(portfolio);
            await _db.SaveChangesAsync();
            return portfolio;
        }

        public async Task<Portfolio> DeleteAsync(ApplicationUser appUser, string symbol)
        {
            var portfolio = await _db.Portfolios.FirstOrDefaultAsync(x => (x.Stock.Symbol.ToLower() == symbol.ToLower()) && x.UserId == appUser.Id);
            if (portfolio is null)
                return null;

            _db.Portfolios.Remove(portfolio);
            await _db.SaveChangesAsync();
            return portfolio;
        }

  
        
        public async Task<List<Stock>> GetUserPortfolio(ApplicationUser user)
        {
            return await _db.Portfolios.Where(x => x.UserId == user.Id).Select(x => new Stock()
            {
                Id = x.StockId,
                Symbol = x.Stock.Symbol,
                CompanyName = x.Stock.CompanyName,
                Industry = x.Stock.Industry,
                Purchase = x.Stock.Purchase,
                LastDiv =x.Stock.LastDiv,
                MarketCap = x.Stock.MarketCap,
            }).ToListAsync();
        }
    }
}
