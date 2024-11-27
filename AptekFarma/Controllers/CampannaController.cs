using AptekFarma.Models;
using AptekFarma.DTO;
using AptekFarma.Context;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampannaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CampannaController(
            AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("GetAllCampannas")]
        public async Task<IActionResult> GetAllCampannas()
        {
            return Ok(await _context.Campanna.Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync());
        }

        [HttpGet("GetCampannaById")]
        public async Task<IActionResult> GetCampannaById(int id)
        {
            var campanna = await _context.Campanna.Include(c => c.EstadoCampanna).FirstOrDefaultAsync(x => x.Id == id);
            var productos = new List<ProductoCampanna>();
            if (campanna != null)
            {
                productos = await _context.ProductoCampanna.Where(x => x.CampannaId == campanna.Id).ToListAsync();
            }

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            return Ok(new { campanna, productos });

        }


        [HttpGet("GetCampannaByIdRankings")]
        public async Task<IActionResult> GetCampannaById(int id, [FromQuery] string userId)
        {
            var campanna = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            var productos = await _context.ProductoCampanna
                .Where(x => x.CampannaId == campanna.Id)
                .ToListAsync();

            var formularios = await _context.FormularioVenta
                .Where(f => f.CampannaID == id)
                .Include(f => f.User).ThenInclude(u => u.Pharmacy)
                .ToListAsync();

            var rankingFarmacias = formularios
                .GroupBy(f => f.User.PharmacyID)
                .Select(g => new
                {
                    PharmacyId = g.Key,
                    PharmacyName = g.FirstOrDefault()?.User.Pharmacy?.Nombre ?? "Sin nombre",
                    TotalVentas = g.Sum(f => f.TotalPuntos)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(3)
                .ToList();

            var farmaciaIdUsuario = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.PharmacyID)
                .FirstOrDefaultAsync();

            var rankingUsuariosFarmacia = formularios
                .Where(f => f.User.PharmacyID == farmaciaIdUsuario)
                .GroupBy(f => f.UserID)
                .Select(g => new
                {
                    UserId = g.Key,
                    UserName = g.FirstOrDefault()?.User?.nombre ?? "Sin nombre",
                    TotalVentas = g.Sum(f => f.TotalPuntos)
                })
                .OrderByDescending(x => x.TotalVentas)
                .Take(3)
                .ToList();

            var formulariosUsuario = await _context.FormularioVenta
                .Where(f => f.CampannaID == id && f.UserID == userId)
                .ToListAsync();


            var ventaProductosUsuario = await (from v in _context.VentaCampanna
                                               join f in _context.FormularioVenta
                                               on v.FormularioID equals f.Id
                                               where f.UserID == userId && f.CampannaID == id
                                               select new
                                               {
                                                   ProductoId = v.ProductoCampanna.Id,
                                                   ProductoNombre = v.ProductoCampanna.Nombre,
                                                   CantidadVendida = v.Cantidad
                                               })
                       .GroupBy(v => v.ProductoId)
                       .Select(g => new
                       {
                           ProductoId = g.Key,
                           ProductoNombre = g.FirstOrDefault().ProductoNombre,
                           CantidadVendida = g.Sum(v => v.CantidadVendida)
                       })
                       .OrderByDescending(x => x.CantidadVendida)
                       .Take(3)
                       .ToListAsync();


            return Ok(new
            {
                campanna,
                productos,
                rankings = new
                {
                    topFarmacias = rankingFarmacias,
                    topUsuariosFarmacia = rankingUsuariosFarmacia,
                    topProductosUsuario = ventaProductosUsuario
                }
            });
        }


        [HttpPost("CreateCampanna")]
        public async Task<IActionResult> CreateCampanna(CrearCampannaDTO campannaDTO)
        {

            var campanna = new Campanna
            {
                Nombre = campannaDTO.nombre,
                Titulo = campannaDTO.titulo,
                Descripcion = campannaDTO.descripcion,
                Importante = campannaDTO.importante,
                Imagen = campannaDTO.imagen,
                FechaInicio = campannaDTO.fechaInicio,
                FechaFin = campannaDTO.fechaFin,
                FechaValido = campannaDTO.fechaValido,
                Activo = true
            };

            // Asignar el estado de la campaña dependiendo de si la fecha actual está entre la fecha de inicio y fin
            if (DateTime.Now.Date >= campanna.FechaInicio.Date && DateTime.Now.Date <= campanna.FechaFin.Date)
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 1);
            }
            else
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 2);
            }

            await _context.Campanna.AddAsync(campanna);
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync();

            return Ok(new { message = "Campaña creada correctamente", campannas });
        }

        [HttpPut("UpdateCampanna")]
        public async Task<IActionResult> UpdateCampanna([FromBody] UpdateCampannaDTO campannaDTO)
        {
            var campanna = await _context.Campanna
                .FirstOrDefaultAsync(x => x.Id == campannaDTO.id);


            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            campanna.Nombre = campannaDTO.nombre;
            campanna.Titulo = campannaDTO.titulo;
            campanna.Importante = campannaDTO.importante;
            campanna.Descripcion = campannaDTO.descripcion;
            campanna.FechaInicio = campannaDTO.fechaInicio;
            campanna.FechaFin = campannaDTO.fechaFin;
            campanna.FechaValido = campannaDTO.fechaValido;

            if(campannaDTO.imagen != null)
            {
                campanna.Imagen = campannaDTO.imagen;
            }

            // Asignar el estado de la campaña dependiendo de si la fecha actual está entre la fecha de inicio y fin
            if (DateTime.Now.Date >= campanna.FechaInicio.Date && DateTime.Now.Date <= campanna.FechaFin.Date)
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 1);
            }
            else
            {
                campanna.EstadoCampanna = await _context.EstadoCampanna.FirstOrDefaultAsync(x => x.Id == 2);
            }


            _context.Campanna.Update(campanna);
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna
                .Where(c => c.Activo == true)
                .Include(c => c.EstadoCampanna).ToListAsync();

            return Ok(new { message = "Campaña editada correctamente", campannas });
        }

        [HttpDelete("DeleteCampanna")]
        public async Task<IActionResult> DeleteCampanna(int id)
        {
            var campanna = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            campanna.Activo = false;
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync();
            return Ok(new { message = "Eliminada Correctamente", campannas });
        }
        [HttpGet("GetCampannaInformes")]
        public async Task<IActionResult> GetCampanna([FromQuery] string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                return BadRequest("Debe proporcionar un UserID.");
            }

            var user = await _context.Users.FindAsync(userID);
            if (user == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var campannas = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .Where(c => c.Activo == true)
                .ToListAsync();

            var campannaDTOs = new List<CampannaDTO>();
            if (campannas == null || !campannas.Any())
            {
                return Ok(campannaDTOs);
            }

            foreach (var campanna in campannas)
            {
                // Filtrar formularios por CampannaID y UserID
                var formularios = await _context.FormularioVenta
                    .Where(f => f.CampannaID == campanna.Id && f.UserID == userID)
                    .ToListAsync();

                var formulariosNoValidados = formularios
                    .Where(f => f.EstadoFormularioID == 1)
                    .ToList();

                var formulariosValidados = formularios
                    .Where(f => f.EstadoFormularioID == 2)
                    .ToList();

                var campannaDTO = new CampannaDTO
                {
                    id = campanna.Id,
                    nombre = campanna.Nombre,
                    titulo = campanna.Titulo,
                    imagen = campanna.Imagen,
                    importante = campanna.Importante,
                    descripcion = campanna.Descripcion,
                    fechaInicio = campanna.FechaInicio,
                    fechaFin = campanna.FechaFin,
                    fechaValido = campanna.FechaValido,
                    estadoCampanna = campanna.EstadoCampanna,
                    informesPendientes = formulariosNoValidados.Count,
                    informesConfirmados = formulariosValidados.Count,
                    puntosObtenidos = formulariosValidados.Sum(f => f.TotalPuntos)
                };

                campannaDTOs.Add(campannaDTO);
            }

            return Ok(campannaDTOs);
        }



    }
}
