

using Data.Models;
using Services.Models.Dtos.Stock;
using Services.Models.Helper;


namespace Services.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<List<Stock>> GetAllAsync(QueryObject query);

        Task<Stock?> GetByIdAsync(int id);
        Task<Stock?> GetBySybmbolAsync(string symbol); 

        Task<Stock?> CreateAsync(Stock stockModel);

        Task<Stock?> UpdateAsync(int id, UpdateStockDto stockDto);

        Task<Stock?> DeleteAsync(int id);

        //hàm để check cho StockId có tồn tại hay không 
        Task<bool> IsStockIdExist(int id);



    }
}
