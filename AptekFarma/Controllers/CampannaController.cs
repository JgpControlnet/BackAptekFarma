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
           return Ok(await _context.Campanna.Include(c => c.EstadoCampanna).ToListAsync());
        }

        [HttpGet("GetCampannaById")]
        public async Task<IActionResult> GetCampannaById(int id)
        {
            var campanna = await _context.Campanna.Include(c=> c.EstadoCampanna).FirstOrDefaultAsync(x => x.Id == id);
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

        [HttpPost("CreateCampanna")]
        public async Task<IActionResult> CreateCampanna(CampannaDTO campannaDTO)
        {

            var campanna = new Campanna
            {
                Nombre = campannaDTO.nombre,
                Titulo = campannaDTO.titulo,
                Descripcion = campannaDTO.descripcion,
                Importante = campannaDTO.importante,
                FechaInicio = campannaDTO.fechaInicio,
                FechaFin = campannaDTO.fechaFin,
                FechaValido = campannaDTO.fechaValido,
                EstadoCampannaId = 1
            };

            await _context.Campanna.AddAsync(campanna);
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna).ToListAsync();

            return Ok(new { message = "Campaña creada correctamente", campannas });
        }

        [HttpPut("UpdateCampanna")]
        public async Task<IActionResult> UpdateCampanna([FromBody] CampannaDTO campannaDTO)
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
            campanna.EstadoCampannaId = campannaDTO.estadoCampanna.Id;


            _context.Campanna.Update(campanna);
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna).ToListAsync();

            return Ok(new { message = "Campaña editada correctamente", campannas });
        }

        [HttpDelete("DeleteCampanna")]
        public async Task<IActionResult> DeleteCampanna(int id)
        {
            var campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == id);

            if (campanna == null)
            {
                return NotFound(new { message = "No se ha encontrado Campaña" });
            }

            _context.Campanna.Remove(campanna);
            await _context.SaveChangesAsync();
            var campannas = await _context.Campanna.Include(c => c.EstadoCampanna).ToListAsync();
            return Ok(new { message = "Eliminada Correctamente", campannas });
        }
        [HttpGet("GetCampannaInformes")]
        public async Task<IActionResult> GetCampanna()
        {
            var campannas = await _context.Campanna
                .Include(c => c.EstadoCampanna)
                .ToListAsync();

            if (campannas == null || !campannas.Any())
            {
                return NotFound("No se encontraron campañas.");
            }

            var campannaDTOs = new List<CampannaDTO>();

            foreach (var campanna in campannas)
            {
                var formularios = await _context.FormularioVenta
                    .Where(f => f.CampannaID == campanna.Id)
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
                    importante = campanna.Importante,
                    descripcion = campanna.Descripcion,
                    fechaInicio = campanna.FechaInicio,
                    fechaFin = campanna.FechaFin,
                    fechaValido = campanna.FechaValido,
                    estadoCampanna = campanna.EstadoCampanna
                };

                 campannaDTO.informesPendientes = formulariosNoValidados.Count;
                 campannaDTO.informesConfirmados = formulariosValidados.Count;
                 campannaDTO.puntosObtenidos = formulariosValidados.Sum(f => f.TotalPuntos);
                campannaDTOs.Add(campannaDTO);
            }

            return Ok(campannaDTOs);
        }


    }
}
