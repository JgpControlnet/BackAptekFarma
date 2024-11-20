using AptekFarma.Models;
using AptekFarma.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AptekFarma.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using AptekFarma.Migrations;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FormularioVentaCampannaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public FormularioVentaCampannaController(
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
        [HttpGet("GetAllformularios")]
        public IActionResult GetAllformularios(
            [FromQuery] int? campannaID,
            [FromQuery] string? userID,
            [FromQuery] int? EstadoFormularioID)
        {
            var query = _context.FormularioVenta
                .Include(f => f.User)
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

            // Ejecutar la consulta y mapear a DTO
            var formularios = query.ToList();
            var formulariosDTO = formularios.Select(f => new FormularioVentaDTO
            {
                id = f.Id,
                userID = f.UserID,
                user = new UserDTO
                {
                    Id = f.User.Id,
                    Nombre = f.User.nombre,
                    Points = f.User.Points
                },
                estadoFormularioID = f.EstadoFormularioID,
                estadoFormulario = f.EstadoFormulario,
                campannaID = f.CampannaID,
                campanna = f.Campanna,
                ventaCampannas = _context.VentaCampanna
                    .Include(vc => vc.ProductoCampanna)
                    .Where(vc => vc.FormularioID == f.Id)
                    .ToList(),
                totalPuntos = f.TotalPuntos
            });

            return Ok(formulariosDTO);
        }


        //get formulario by id
        [HttpGet("GetFormulario")]
        public async Task<IActionResult> GetFormulario([FromQuery] int formularioID)
        {
            var formulario = await _context.FormularioVenta
                .Include(f => f.User)
                .Include(f => f.Campanna)
                .Include(f => f.EstadoFormulario)
                .FirstOrDefaultAsync(f => f.Id == formularioID);

            var ventaCampannas = _context.VentaCampanna
                .Include(vc => vc.ProductoCampanna)
                .Where(vc => vc.FormularioID == formularioID)
                .ToList();

            if (formulario == null)
            {
                return NotFound("Formulario no encontrado.");
            }

            var formularioDTO = new FormularioVentaDTO
            {
                id = formulario.Id,
                userID = formulario.UserID,
                user = new UserDTO
                {
                    Id = formulario.User.Id,
                    Nombre = formulario.User.nombre,
                    Points = formulario.User.Points
                },
                estadoFormularioID = formulario.EstadoFormularioID,
                estadoFormulario = formulario.EstadoFormulario,
                campannaID = formulario.CampannaID,
                campanna = formulario.Campanna,
                ventaCampannas = ventaCampannas,
                totalPuntos = formulario.TotalPuntos
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

                var puntosProducto = producto.Cantidad * productoCampanna.Puntos;
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
                EstadoFormularioID = 2 
            };

            // Guarda el formulario
            _context.FormularioVenta.Add(formularioVenta);
            await _context.SaveChangesAsync();

            // Asocia las ventas al formulario
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

            var ventaCampannas = _context.VentaCampanna
                   .Include(vc => vc.ProductoCampanna)
                   .Where(vc => vc.FormularioID == requestValidar.formularioID)
                   .ToList();

            if (formulario == null)
            {
                return NotFound("Formulario no encontrado.");
            }

            if (formulario.EstadoFormularioID == 2)
            {
                return BadRequest("El formulario ya ha sido validado.");
            }

            double totalPuntosFormulario = 0;

            foreach (var venta in ventaCampannas)
            {
                var producto = venta.ProductoCampanna;
                int cantidadCanjeada = Math.Min(venta.Cantidad, producto.UnidadesMaximas);
                totalPuntosFormulario += cantidadCanjeada * producto.Puntos;
            }

            formulario.User.Points += totalPuntosFormulario;

            formulario.EstadoFormularioID = 2;

            _context.Update(formulario);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Formulario de venta creado con éxito"});

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
       
    }
}
