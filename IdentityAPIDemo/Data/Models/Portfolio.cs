using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Models
{
    public class Portfolio
    {
        [Key, Column(Order = 0)]
        public string? UserId { get; set; }
        public ApplicationUser User { get; set; }
        [Key, Column(Order = 1)]
        public int StockId { get; set; }
        public Stock Stock { get; set; }
    }
}
