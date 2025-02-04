using AptekFarma.Models;
using AptekFarma.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AptekFarma.DTO;
using AptekFarma.Services;


namespace AptekFarma.Controllers
{
    [Route("rest/[controller]")]
    [ApiController]
    [Authorize]
    public class VentaPuntosController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public VentaPuntosController(
            UserManager<User> userManager,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
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
            ventas = ventas.OrderByDescending(x => x.FechaCompra).ToList();

            if (filtro.Todas)
            {

                var send = ventas.Select(ventas => new
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
            ventas = ventas.OrderByDescending(x => x.FechaCompra).ToList();
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

            var html = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #003366; border-radius: 10px; background-color: #f8f9fa;'>
                    <div style='background-color: #003366; color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h2 style='margin: 0;'>Comprobante de Compra</h2>
                        <p style='margin: 5px 0;'>Fecha: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</p>
                    </div>
                    <div style='background-color: white; padding: 20px; border-radius: 0 0 10px 10px;'>
                        <div style='border-bottom: 2px solid #003366; padding-bottom: 10px; margin-bottom: 15px;'>
                            <p style='margin: 5px 0;'><strong>Cliente:</strong> {user.UserName}</p>
                            <p style='margin: 5px 0;'><strong>ID Venta:</strong> {venta.Id}</p>
                        </div>
                        <table style='width: 100%; border-collapse: collapse; margin-bottom: 15px;'>
                            <thead>
                                <tr style='background-color: #e9ecef;'>
                                    <th style='padding: 10px; text-align: left; border-bottom: 2px solid #dee2e6;'>Producto</th>
                                    <th style='padding: 10px; text-align: center; border-bottom: 2px solid #dee2e6;'>Cantidad</th>
                                    <th style='padding: 10px; text-align: right; border-bottom: 2px solid #dee2e6;'>Puntos por unidad</th>
                                    <th style='padding: 10px; text-align: right; border-bottom: 2px solid #dee2e6;'>Total Puntos</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{product.Nombre}</td>
                                    <td style='padding: 10px; text-align: center; border-bottom: 1px solid #dee2e6;'>{dto.cantidad}</td>
                                    <td style='padding: 10px; text-align: right; border-bottom: 1px solid #dee2e6;'>{product.PuntosNecesarios}</td>
                                    <td style='padding: 10px; text-align: right; border-bottom: 1px solid #dee2e6;'>{puntosTotalesCompra}</td>
                                </tr>
                            </tbody>
                        </table>
                        <div style='background-color: #e9ecef; padding: 15px; border-radius: 5px;'>
                            <p style='margin: 5px 0; text-align: right;'><strong>Total Puntos Gastados:</strong> {puntosTotalesCompra}</p>
                            <p style='margin: 5px 0; text-align: right;'><strong>Puntos Restantes:</strong> {user.Points}</p>
                        </div>
                        <div style='text-align: center; margin-top: 20px; color: #003366;'>
                            <p style='margin: 5px 0;'>¡Gracias por su compra!</p>
                            <p style='margin: 5px 0;'>Este comprobante sirve como recibo de su transacción.</p>
                        </div>
                    </div>
                </div>";

            var warehouseEmail = "testing@controlnet.es";

            var warehouseHtml = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #003366; border-radius: 10px; background-color: #f8f9fa;'>
                    <div style='background-color: #003366; color: white; padding: 20px; border-radius: 10px 10px 0 0; text-align: center;'>
                        <h2 style='margin: 0;'>Detalles de Venta para Almacén</h2>
                        <p style='margin: 5px 0;'>Fecha: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}</p>
                    </div>
                    <div style='background-color: white; padding: 20px; border-radius: 0 0 10px 10px;'>
                        <table style='width: 100%; border-collapse: collapse; margin-bottom: 15px;'>
                            <tr>
                                <th style='padding: 10px; text-align: left; background-color: #e9ecef; border-bottom: 2px solid #dee2e6;'>Concepto</th>
                                <th style='padding: 10px; text-align: left; background-color: #e9ecef; border-bottom: 2px solid #dee2e6;'>Detalle</th>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>ID de Venta</strong></td>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{venta.Id}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Nombre del Producto</strong></td>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{product.Nombre}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Código del Producto</strong></td>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{product.CodProducto}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Cantidad Vendida</strong></td>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{dto.cantidad}</td>
                            </tr>
                            <tr>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'><strong>Stock Restante</strong></td>
                                <td style='padding: 10px; border-bottom: 1px solid #dee2e6;'>{product.CantidadMax}</td>
                            </tr>
                        </table>
                        <div style='background-color: #e9ecef; padding: 15px; border-radius: 5px; text-align: center;'>
                            <p style='margin: 5px 0;'>Este es un aviso automático. Por favor, actualice el inventario según sea necesario.</p>
                        </div>
                    </div>
                </div>";

            var emailService = new EmailService(_configuration);
            await emailService.SendEmailAsync(user.Email, "Comprobante de Compra", html);
            await emailService.SendEmailAsync(warehouseEmail, "Detalles de Venta para Almacén", warehouseHtml);

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
            ventasDTO = ventasDTO.OrderByDescending(v => v.FechaCompra).ToList();

            return Ok(ventasDTO);
        }


    }
}
