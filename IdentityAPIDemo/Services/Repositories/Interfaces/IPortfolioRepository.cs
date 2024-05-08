

using Data.Models;

namespace Services.Repositories.Interfaces
{
    public interface IPortfolioRepository
    {
        Task<List<Stock>> GetUserPortfolio(ApplicationUser user);

        Task<Portfolio> CreateAsync(Portfolio portfolio);
        Task<Portfolio> DeleteAsync(ApplicationUser appUser, string symbol  );

        

    }
}
