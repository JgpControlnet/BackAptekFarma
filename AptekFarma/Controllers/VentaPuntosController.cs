using _AptekFarma.Models;
using _AptekFarma.DTO;
using _AptekFarma.Context;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AptekFarma.Models;
using OfficeOpenXml;
using AptekFarma.Controllers;
using AptekFarma.DTO;


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class VentaPuntosController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public VentaPuntosController(
            UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("GetAllVentas")]
        public async Task<IActionResult> GetVentas([FromBody] VentaPuntosDTO filtro)
        {
            var ventas = await _context.VentaPuntos
                .Include(x => x.Product)
                .Include(x => x.User)
                .ToListAsync();

            if (filtro.Todas) {

                var send = ventas.Select(ventas => new
                {
                    Id = ventas.Id,
                    User = _context.Users
                    .Include(u => u.Pharmacy)
                    .Where(u => u.Id == ventas.UserID)
                    .Select(u => new { 
                        u.nombre,
                        u.apellidos,
                        u.Pharmacy.Nombre
                    }),
                    Product = ventas.Product,
                    Cantidad = ventas.Cantidad,
                    PuntosTotales = ventas.PuntosTotales,
                    FechaCompra = ventas.FechaCompra
                }).ToList();
                
                return Ok(ventas);
            }
               

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.UserID))
                {
                    ventas = ventas.Where(x => x.UserID == filtro.UserID).ToList();
                }

                if (filtro.ProductID > 0)
                {
                    ventas = ventas.Where(x => x.ProductID == filtro.ProductID).ToList();
                }

                if (filtro.PuntosTotales > 0)
                {
                    ventas = ventas.Where(x => x.PuntosTotales == filtro.PuntosTotales).ToList();
                }

                if (filtro.FechaCompra != null)
                {
                    ventas = ventas.Where(x => x.FechaCompra == filtro.FechaCompra).ToList();

                }
            }

            // Paginación
            int totalItems = ventas.Count;
            var paginatedVentas = ventas
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .Select(ventas => new
                {
                    Id = ventas.Id,
                    User = _context.Users
                    .Include(u => u.Pharmacy)
                    .Where(u => u.Id == ventas.UserID)
                    .Select(u => new {
                        u.nombre,
                        u.apellidos,
                        u.Pharmacy.Nombre
                    }),
                    Product = ventas.Product,
                    Cantidad = ventas.Cantidad,
                    PuntosTotales = ventas.PuntosTotales,
                    FechaCompra = ventas.FechaCompra
                }).ToList();

            return Ok(ventas);

        }

        [HttpGet("GetVentaById")]
        public async Task<IActionResult> GetVentaById(int id)
        {
            var venta = await _context.VentaPuntos
                .Include(x => x.Product)
                .Include(x => x.User)
                    .ThenInclude(x => x.Pharmacy)
                    .Select(ventas => new
                    {
                        Id = ventas.Id,
                        User = _context.Users
                    .Include(u => u.Pharmacy)
                    .Where(u => u.Id == ventas.UserID)
                    .Select(u => new
                    {
                        u.nombre,
                        u.apellidos,
                        u.Pharmacy.Nombre
                    }),
                        Product = ventas.Product,
                        Cantidad = ventas.Cantidad,
                        PuntosTotales = ventas.PuntosTotales,
                        FechaCompra = ventas.FechaCompra
                    })
                .FirstOrDefaultAsync(x => x.Id == id);

            if (venta == null)
                return NotFound();

            return Ok(venta);
        }

        [HttpPost("ComprarProducto")]
        public async Task<IActionResult> ComprarProducto([FromBody] CompraPuntosDTO dto)
        {
            var product = _context.ProductVenta.FirstOrDefault(x => x.Id == dto.idProducto);

            if(product == null)
                return NotFound(new { message = "Producto no encontrado" });

            if (product.CantidadMax < dto.cantidad)
                return BadRequest(new { message = "No hay suficiente stock" });

            var user = await _userManager.FindByIdAsync(dto.idUsuario.ToString());
            
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var venta = new VentaPuntos
            {
                UserID = user.Id,
                ProductID = product.Id,
                Cantidad = dto.cantidad,
                PuntosTotales = product.PuntosNecesarios * dto.cantidad,
                FechaCompra = DateTime.Now
            };

            product.CantidadMax -= dto.cantidad;

            _context.ProductVenta.Update(product);
            await _context.VentaPuntos.AddAsync(venta);
            
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Compra realizada correctamente" });
        }



    }
}
