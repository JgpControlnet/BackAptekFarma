using AptekFarma.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AptekFarma.Models;

namespace AptekFarma.Context
{
    public class AppDbContext: IdentityDbContext<User, Roles, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Pharmacy> Pharmacy { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Campanna> Campanna { get; set; }
        public DbSet<FormularioVentaCampanna> FormularioVenta { get; set; }
        public DbSet<VentaCampanna> VentaCampanna { get; set; }
        public DbSet<ProductoCampanna> ProductoCampanna { get; set; }
        public DbSet<ProductoVenta> ProductVenta { get; set; }
        public DbSet<VentaPuntos> VentaPuntos { get; set; }
        public DbSet<EstadoCampanna> EstadoCampanna { get; set; }
        public DbSet<EstadoFormulario> EstadoFormulario { get; set; }
    }
}
