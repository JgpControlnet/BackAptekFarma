using _AptekFarma.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AptekFarma.Models;

namespace _AptekFarma.Context
{
    public class AppDbContext: IdentityDbContext<User, Roles, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Products> Products { get; set; }
        public DbSet<Campaigns> Campaigns { get; set; }
        public DbSet<Pharmacy> Pharmacies { get; set; }
        public DbSet<Sales> Sales { get; set; }
        public DbSet<PointEarned> PointsEarned { get; set; }
        public DbSet<PointRedeemded> PointsRedeemded { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

    }
}
