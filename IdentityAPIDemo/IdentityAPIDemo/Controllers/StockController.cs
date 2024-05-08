
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Services.Mappers;
using Services.Models.Dtos.Stock;
using Services.Models.Helper;
using Services.Repositories.Interfaces;

namespace IdentityAPIDemo.Controllers
{
    [Route("api/stock")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly IStockRepository _stockRepo;

        public StockController(IStockRepository stockRepo)
        {
            _stockRepo = stockRepo;            
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] QueryObject query)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stocks = await _stockRepo.GetAllAsync(query);

            var stocksDto = stocks.Select(s => s.ToStockDto()).ToList();

            return Ok(stocksDto);
        }

        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stock = await _stockRepo.GetByIdAsync(id);
            if (stock == null)
                return NotFound();
            return Ok(stock.ToStockDto());
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CreateStockDto stockDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = stockDto.ToStockFromCreateDto();
            var addedModel = await _stockRepo.CreateAsync(stockModel);
            //trả về 1 hành động chạy hàm getbyId để trả về kết quả được tạo
            //return CreatedAtAction(nameof(GetById), new { id = stockModel.Id }, stockModel.ToStockDto());
            return Ok(addedModel);

        }

        [HttpPut]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateStockDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = await _stockRepo.UpdateAsync(id, updateDto);
            if (stockModel is null)
                return NotFound();

            return Ok(stockModel);

        }

        [HttpDelete]
        [Route("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var stockModel = await _stockRepo.DeleteAsync(id);
            if (stockModel is null)
                return NotFound();

            return NoContent();
        }
    }
}
