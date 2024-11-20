using AptekFarma.Models;
using AptekFarma.DTO;
using AptekFarma.Context;

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
using System.Globalization;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CampannaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CampannaController(
            UserManager<User> userManager,
            RoleManager<Roles> roleManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
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
                FechaInicio = campannaDTO.fechaInicio,
                FechaFin = campannaDTO.fechaFin,
                EstadoCampannaId = 1 // Estado activo
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
            campanna.Descripcion = campannaDTO.descripcion;
            campanna.FechaInicio = campannaDTO.fechaInicio;
            campanna.FechaFin = campannaDTO.fechaFin;
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
    }
}
