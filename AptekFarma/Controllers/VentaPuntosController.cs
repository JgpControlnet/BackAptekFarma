using AptekFarma.Models;
using AptekFarma.Context;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AptekFarma.DTO;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VentaPuntosController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public VentaPuntosController(
            UserManager<User> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
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
            if (product == null)
                return NotFound(new { message = "Producto no encontrado" });

            if (product.CantidadMax < dto.cantidad)
                return BadRequest(new { message = "No hay suficiente stock disponible para la compra" });

            var user = await _userManager.FindByIdAsync(dto.idUsuario.ToString());
            if (user == null)
                return NotFound(new { message = "Usuario no encontrado" });

            var puntosTotalesCompra = product.PuntosNecesarios * dto.cantidad;

            if (user.Points < (double)puntosTotalesCompra)
                return BadRequest(new { message = "El usuario no tiene suficientes puntos para realizar esta compra" });

            var venta = new VentaPuntos
            {
                UserID = user.Id,
                ProductID = product.Id,
                Cantidad = dto.cantidad,
                PuntosTotales = puntosTotalesCompra,
                FechaCompra = DateTime.Now
            };

            user.Points -= (double)puntosTotalesCompra;

            product.CantidadMax -= dto.cantidad;

            _context.ProductVenta.Update(product);
            _context.VentaPuntos.Add(venta);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Compra realizada correctamente",
                puntosRestantes = user.Points,
                cantidadRestante = product.CantidadMax
            });
        }

        [HttpGet("GetVentasPorUsuario")]
        public async Task<IActionResult> GetVentasPorUsuario([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("El ID del usuario es requerido.");
            }

            var ventas = await _context.VentaPuntos
                .Include(v => v.User) 
                .Include(v => v.Product) 
                .Where(v => v.UserID == userId) 
                .OrderByDescending(v => v.FechaCompra)
                .ToListAsync();

            if (ventas == null || !ventas.Any())
            {
                return NotFound("No se encontraron ventas para el usuario proporcionado.");
            }

            var ventasDTO = ventas.Select(v => new
            {
                v.Id,
                v.UserID,
                Usuario = v.User?.UserName,
                v.ProductID,
                Producto = v.Product?.Nombre,
                v.Cantidad,
                v.PuntosTotales,
                v.FechaCompra
            });

            return Ok(ventasDTO);
        }


    }
}
