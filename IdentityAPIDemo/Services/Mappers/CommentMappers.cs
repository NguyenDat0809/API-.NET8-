
using Data.Models;
using Services.Models.Dtos.Comment;

namespace Services.Mappers
{
    public static class CommentMappers
    {
        public static CommentDto ToCommentDto(this Comment commentModel)
        {
            return new CommentDto
            {
                Id = commentModel.Id,
                Tiltle = commentModel.Tiltle,
                Content = commentModel.Content,
                CreateOn = commentModel.CreateOn,
                CreatedBy = commentModel.User.UserName,
                //StockId = commentModel.StockId,
            };
        }

        public static Comment ToCommentFromCreateDto(this CreateCommentDto commentDto, int stockId)
        {
            return new Comment
            {
                Tiltle = commentDto.Tiltle,
                Content = commentDto.Content,
                StockId = stockId,
            };
        }

     

        public static Comment ToCommentFromUpdateDto(this UpdateCommentDto commentDto)
        {
            return new Comment
            {
                Tiltle = commentDto.Tiltle,
                Content = commentDto.Content,
            };
        }
    }
}
