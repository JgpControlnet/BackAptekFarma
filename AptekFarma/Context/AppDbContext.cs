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

        
        public DbSet<Pharmacy> Pharmacy { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Campanna> Campanna { get; set; }
        public DbSet<FormularioVenta> FormularioVenta { get; set; }
        public DbSet<ProdcutoCampanna> ProdcutoCampanna { get; set; }
        public DbSet<ProductVenta> ProductVenta { get; set; }
        public DbSet<VentaPuntos> VentaPuntos { get; set; }





        // public DbSet<Localidad> Localidades { get; set; }
        // public DbSet<Provincia> Provincias { get; set; }    

    }
}
