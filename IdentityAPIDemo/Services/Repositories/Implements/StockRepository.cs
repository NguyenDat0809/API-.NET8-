
using Data.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Models.Dtos.Stock;
using Services.Models.Helper;
using Services.Repositories.Interfaces;

namespace FinShark.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly ApplicationDbContext _db;

        public StockRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Stock>> GetAllAsync(QueryObject query)
        {
            //biến câu truy vấn này thành QueryAble để tiếp tục truy vấn
            var stocks = _db.Stocks.Include(x => x.Comments)
                .ThenInclude(x => x.User)
                 .AsQueryable();
            //FILTERS
            //theo CompanyName
            if (!string.IsNullOrEmpty(query?.CompanyName?.Trim()))
                stocks = stocks.Where(s => s.CompanyName.Contains(query.CompanyName));
            //theo Symbol
            if (!string.IsNullOrEmpty(query?.Symbol?.Trim()))
                stocks = stocks.Where(s => s.Symbol.Contains(query.Symbol));
            //Sorting
            if (!string.IsNullOrEmpty(query?.SortBy?.Trim()))
                if (query.SortBy.Equals("Symbol", StringComparison.OrdinalIgnoreCase))
                    stocks = query.IsDecsending ? stocks.OrderByDescending(s => s.Symbol) : stocks.OrderBy(s => s.Symbol);
            //Pagination
            var skipRecord = (query.PageNumber - 1) * query.PageSize;
            stocks = stocks.Skip(skipRecord).Take(query.PageSize);

            return await stocks.ToListAsync();
        }

        public async Task<Stock?> GetByIdAsync(int id)
        {
           return await _db.Stocks.Include(x => x.Comments).ThenInclude(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
            
        }
        /// <summary>
        /// Hàm tạo 1 stock mới.
        /// </summary>
        /// <param name="stockModel"></param>
        /// <returns>trả về thông tin stock vừa được tạo mới</returns>
        public async Task<Stock?> CreateAsync(Stock stockModel)
        {
            await _db.Stocks.AddAsync(stockModel);
            await _db.SaveChangesAsync();
            return stockModel;
        }
        /// <summary>
        /// Hàm update 1 stock.
        /// hàm kiểm tra xem stock có tồn tại với key id đưa vào hay không.
        /// Nếu không trả về null, nếu có thì tiến hành update stock đó
        /// </summary>
        /// <param name="id"></param>
        /// <param name="stockDto"></param>
        /// <returns>null nếu tìm không thấy record || thông tin stock vừa được update nếu thành công</returns>
        public async Task<Stock?> UpdateAsync(int id, UpdateStockDto stockDto)
        {
            var stockModel = await GetByIdAsync(id);
            if (stockModel is null)
                return null;
            stockModel.Symbol = stockDto.Symbol;
            stockModel.CompanyName = stockDto.CompanyName;
            stockModel.LastDiv = stockDto.LastDiv;
            stockModel.Purchase = stockDto.Purchase;
            stockModel.Industry = stockDto.Industry;
            stockModel.MarketCap = stockDto.MarketCap;
            await _db.SaveChangesAsync();
            return stockModel;
        }
        /// <summary>
        /// Hàm xóa 1 stock.
        /// Hàm nhận vào id, tiến hành kiểm tra xem có record nào tồn tại với key id không
        /// Nếu không thì trả về null, nếu có thì tiến hành remove và trả về thông tin stock đã bị xóa
        /// </summary>
        /// <param name="id"></param>
        /// <returns>null nếu tìm không thấy record || thông tin stock vừa được update nếu thành công</returns>
        public async Task<Stock?> DeleteAsync(int id)
        {
            var stockModel = await GetByIdAsync(id);
            if (stockModel is null)
                return null;
            _db.Stocks.Remove(stockModel);
            await _db.SaveChangesAsync();
            return stockModel;
        }
        public async Task<bool> IsStockIdExist(int id)
        {
            return await _db.Stocks.AnyAsync(s => s.Id == id);
        }

        public async Task<Stock?> GetBySybmbolAsync(string symbol)
        {
            return await _db.Stocks.FirstOrDefaultAsync(x => x.Symbol == symbol);
        }
    }
}
