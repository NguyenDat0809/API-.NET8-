using System.ComponentModel.DataAnnotations;

namespace Services.Models.Dtos.Comment
{
    public class UpdateCommentDto
    {
        [Required]
        [MinLength(5, ErrorMessage = "Title must be 5 charatcters")]
        [MaxLength(280, ErrorMessage = "Title can not be over 280 charatcters")]
        public string? Tiltle { get; set; } = string.Empty;
        [Required]
        [MinLength(5, ErrorMessage = "Content must be 5 charatcters")]
        [MaxLength(280, ErrorMessage = "Content can not be over 280 charatcters")]
        public string? Content { get; set; } = string.Empty;

        //public int? StockId { get; set; }
    }
}