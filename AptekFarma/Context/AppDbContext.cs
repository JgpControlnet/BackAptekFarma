using _AptekFarma.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AptekFarma.Models;

namespace _AptekFarma.Context
{
    public class AppDbContext: IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Products> Products { get; set; }
        public DbSet<Campaigns> Campaigns { get; set; }

    }
}
