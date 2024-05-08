using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Stock> Stocks { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }
       

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //create many-many relation ship
            //định nghĩa primary key với khóa đôi
            builder.Entity<Portfolio>().HasKey(p => new { p.UserId, p.StockId });

            //GIẢI NGHĨA: mỗi Stock xuất hiện nhiều trong Portfolio, và mỗi Portfolio được liên kết với một Stock thông qua khóa ngoại Stockid.
            builder.Entity<Portfolio>()
                .HasOne(o => o.Stock)
                .WithMany(m => m.Portfolios)
                .HasForeignKey(f => f.StockId);

            builder.Entity<Portfolio>()
                .HasOne(o => o.User)
                .WithMany(m => m.Portfolios)
                .HasForeignKey(f => f.UserId);


            SeedRoles(builder);
        }

        private void SeedRoles(ModelBuilder builder)
        {
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole() { Name = "Admin", ConcurrencyStamp = "1", NormalizedName = "Admin" },
                new IdentityRole() { Name = "User", ConcurrencyStamp = "2", NormalizedName = "User" },
                new IdentityRole() { Name = "HR", ConcurrencyStamp = "3", NormalizedName = "Human Resource" }
                );
        }

    }
}
