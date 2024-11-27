using AptekFarma.Models;
using AptekFarma.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AptekFarma.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using AptekFarma.Migrations;
using static Microsoft.IO.RecyclableMemoryStreamManager;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FormularioVentaCampannaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public FormularioVentaCampannaController(
            UserManager<User> userManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("GetAllformularios")]
        public IActionResult GetAllformularios(
            [FromQuery] int? campannaID,
            [FromQuery] string? userID,
            [FromQuery] int? EstadoFormularioID)
        {
            var query = _context.FormularioVenta
                .Include(f => f.User)
                .ThenInclude(u => u.Pharmacy)
                .Include(f => f.Campanna)
                .Include(f => f.EstadoFormulario)
                .AsQueryable();

            if (campannaID.HasValue)
            {
                query = query.Where(f => f.CampannaID == campannaID.Value);
            }

            if (!string.IsNullOrEmpty(userID))
            {
                query = query.Where(f => f.UserID == userID);
            }

            if (EstadoFormularioID.HasValue)
            {
                query = query.Where(f => f.EstadoFormularioID == EstadoFormularioID.Value);
            }

            var formularios = query.ToList();

            var formulariosDTO = formularios.Select(f => new FormularioVentaDTO
            {
                id = f.Id,
                userID = f.UserID,
                user = f.User,
                estadoFormularioID = f.EstadoFormularioID,
                estadoFormulario = f.EstadoFormulario,
                campannaID = f.CampannaID,
                campanna = f.Campanna,
                ventaCampannas = _context.VentaCampanna
                    .Include(vc => vc.ProductoCampanna)
                    .Where(vc => vc.FormularioID == f.Id)
                    .ToList(),
                totalPuntos = f.TotalPuntos,
                farmacia = f.User.Pharmacy,
                fechaCreacion = f.FechaCreacion
            });

            return Ok(formulariosDTO);
        }
        [HttpGet("GetFormulario")]
        public async Task<IActionResult> GetFormulario([FromQuery] int formularioID)
        {
            var formulario = await _context.FormularioVenta
                .Include(f => f.User)
                .ThenInclude(u => u.Pharmacy)
                .Include(f => f.Campanna)
                .Include(f => f.EstadoFormulario)
                .FirstOrDefaultAsync(f => f.Id == formularioID);

            if (formulario == null)
            {
                return NotFound("Formulario no encontrado.");
            }

            var ventaCampannas = _context.VentaCampanna
                .Include(vc => vc.ProductoCampanna)
                .Where(vc => vc.FormularioID == formularioID)
                .ToList();

            var formularioDTO = new FormularioVentaDTO
            {
                id = formulario.Id,
                userID = formulario.UserID,
                user = formulario.User,
                estadoFormularioID = formulario.EstadoFormularioID,
                estadoFormulario = formulario.EstadoFormulario,
                campannaID = formulario.CampannaID,
                campanna = formulario.Campanna,
                ventaCampannas = ventaCampannas,
                fechaCreacion = formulario.FechaCreacion,
                totalPuntos = formulario.TotalPuntos,
                farmacia = formulario.User.Pharmacy 
            };

            return Ok(formularioDTO);
        }

        [HttpPost("CreateFormularioVenta")]
        public async Task<IActionResult> CreateFormularioVenta([FromBody] FormularioVentaRequest request)
        {
            if (request == null || request.Productos == null || !request.Productos.Any())
            {
                return BadRequest("La solicitud no contiene productos.");
            }

            var user = await _userManager.FindByIdAsync(request.UserID);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var campanna = await _context.Campanna.FindAsync(request.CampannaID);
            if (campanna == null)
            {
                return NotFound("Campaña no encontrada.");
            }

            double totalPuntos = 0;

            var ventas = new List<VentaCampanna>();

            foreach (var producto in request.Productos)
            {
                var productoCampanna = await _context.ProductoCampanna.FindAsync(producto.ProductoCampannaID);
                if (productoCampanna == null)
                {
                    return NotFound($"Producto con ID {producto.ProductoCampannaID} no encontrado.");
                }

                int cantidadCanjeada = Math.Min(producto.Cantidad, productoCampanna.UnidadesMaximas);
                if (cantidadCanjeada == 0)
                {
                    continue;
                }


                var puntosProducto = cantidadCanjeada * productoCampanna.Puntos;
                totalPuntos += puntosProducto;

                ventas.Add(new VentaCampanna
                {
                    PorductoCampannaID = producto.ProductoCampannaID,
                    Cantidad = producto.Cantidad,
                    TotalPuntos = puntosProducto
                });
            }

            var formularioVenta = new FormularioVentaCampanna
            {
                UserID = request.UserID,
                CampannaID = request.CampannaID,
                TotalPuntos = totalPuntos,
                FechaCreacion = DateTime.Now,
                EstadoFormularioID = 1
            };

            _context.FormularioVenta.Add(formularioVenta);
            await _context.SaveChangesAsync();

            foreach (var venta in ventas)
            {
                venta.FormularioID = formularioVenta.Id;
                _context.VentaCampanna.Add(venta);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Formulario de venta creado con éxito", FormularioID = formularioVenta.Id });
        }

        [HttpPost("ValidarFormulario")]
        public async Task<IActionResult> ValidarFormulario([FromBody] RequestValidar requestValidar)
        {
            var formulario = await _context.FormularioVenta
                .Include(f => f.User)
                .Include(f => f.Campanna)
                .Include(f => f.EstadoFormulario)
                .FirstOrDefaultAsync(f => f.Id == requestValidar.formularioID);

            if (formulario == null)
            {
                return NotFound("Formulario no encontrado.");
            }

            if (formulario.EstadoFormularioID == 2)
            {
                return BadRequest("El formulario ya ha sido validado.");
            }

            if (requestValidar.idEstado == 3)
            {
                formulario.EstadoFormulario = await _context.EstadoFormulario.Where(ef => ef.Id == 3).FirstOrDefaultAsync();
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Formulario de venta anulado.", TotalPuntos = 0, formulario = formulario });
            }

            double totalPuntosFormulario = 0;

            foreach (var venta in requestValidar.ventaCampannas)
            {
                var producto = await _context.ProductoCampanna.FindAsync(venta.ProductoCampanna.Id);
                if (producto == null)
                {
                    return NotFound($"Producto con ID {venta.ProductoCampanna.Id} no encontrado.");
                }

                int cantidadCanjeada = Math.Min(venta.Cantidad, producto.UnidadesMaximas);
                if (cantidadCanjeada == 0)
                {
                    continue;
                }

                totalPuntosFormulario += cantidadCanjeada * producto.Puntos;

           
                producto.UnidadesMaximas -= cantidadCanjeada;
                _context.Entry(producto).State = EntityState.Modified;

                venta.Cantidad = cantidadCanjeada;
                venta.TotalPuntos = cantidadCanjeada * producto.Puntos;
                await UpdateVentaCampanna(venta);
            }

            formulario.User.Points += totalPuntosFormulario;
            formulario.EstadoFormularioID = 2;
            formulario.EstadoFormulario = await _context.EstadoFormulario.FindAsync(2);
            formulario.TotalPuntos = totalPuntosFormulario;

            _context.Update(formulario);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Formulario de venta validado con éxito.", TotalPuntos = totalPuntosFormulario, formulario = formulario });
        }


        [HttpPost("UpdateVentaCampanna")]
        public async Task<IActionResult> UpdateVentaCampanna([FromBody] VentaCampanna ventaDTO)
        {
            var ventaCampanna = await _context.VentaCampanna.FindAsync(ventaDTO.Id);

            if (ventaCampanna == null)
            {
                return NotFound("Venta en Campanna no encontrada.");
            }

            ventaCampanna.TotalPuntos = (int)ventaDTO.TotalPuntos;

            _context.Update(ventaCampanna);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Venta en Campanna actualizada con éxito" });
        }
    }

    public class FormularioVentaRequest
    {
        public string UserID { get; set; }
        public int CampannaID { get; set; }
        public List<ProductoRequest> Productos { get; set; }
    }

    public class ProductoRequest
    {
        public int ProductoCampannaID { get; set; }
        public int Cantidad { get; set; }
    }

    public class RequestValidar
    {
        public int formularioID { get; set; }
        public List<VentaCampanna> ventaCampannas { get; set; }
        public int idEstado { get; set; }

    }
}
