using Data.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Mappers;
using Services.Models.Dtos.Comment;
using Services.Repositories.Interfaces;
using Services.ClaimsExtension;

namespace IdentityAPIDemo.Controllers
{
    
    [Route("api/comment")]
    [ApiController]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentRepository _cmRepo;
        private readonly IStockRepository _stockRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ICommentRepository cmRepo, IStockRepository stockRepo, UserManager<ApplicationUser> userManager)
        {
            _cmRepo = cmRepo;
            _stockRepo = stockRepo;
            _userManager = userManager;
        }

        [HttpGet]
        //nhận vào từng property và phân tích
        //link?title=Test
        //link?Content=Hi
        public async Task<IActionResult> GetAll()
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commments = await _cmRepo.GetAllAsync();
            var commentsDto = commments.Select(x => x.ToCommentDto());
            return Ok(commentsDto);
        }

        [HttpGet]
        [Route("get/{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await _cmRepo.GetByIdAsync(id);
            if (comment is null)
                return NotFound();

            return Ok(comment.ToCommentDto());
        }

        [HttpPost("create/{stockId:int}")]
        public async Task<IActionResult> Create([FromRoute] int stockId, [FromBody] CreateCommentDto commentDto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!await _stockRepo.IsStockIdExist(stockId))
                return BadRequest("Stock does not exist");

            var username = User.GetUsername();
            var appUser = await _userManager.FindByNameAsync(username);

            var commentModel = commentDto.ToCommentFromCreateDto(stockId);
            commentModel.UserId = appUser.Id;

            var createdModel = await _cmRepo.CreateAsync(commentModel);
            return CreatedAtAction(nameof(GetById), new {id = createdModel.Id}, createdModel.ToCommentDto());
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var comment = await _cmRepo.DeleteAsync(id);
            if(comment is null)
                return NotFound("Comment does not exist");
            return Ok(comment);
        }
        [HttpPut]
        [Route("update/{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateCommentDto updateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var commentModel = await _cmRepo.UpdateAsync(id, updateDto.ToCommentFromUpdateDto());
            if (commentModel is null)
                return NotFound("Comment not found");

            return Ok(commentModel.ToCommentDto());

        }
    }
}
