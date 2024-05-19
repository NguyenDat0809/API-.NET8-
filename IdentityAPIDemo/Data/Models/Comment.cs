using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string? Tiltle { get; set; } = string.Empty;
        public string? Content { get; set; } = string.Empty;
        public DateTime CreateOn { get; set; } = DateTime.Now;

        
        public int? StockId { get; set; }
        //navigation property
        public Stock? Stock { get; set; }

        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
