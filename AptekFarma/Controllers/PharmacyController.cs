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


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PharmacyController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PharmacyController(
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

        [HttpGet("GetAllPharmacies")]
        public async Task<IActionResult> GetPharmacies([FromQuery] PharmacyFilterDTO filtro)
        {
            var pharmacies = await _context.Pharmacies.ToListAsync();

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Nombre))
                {
                    pharmacies = pharmacies.Where(x => x.Nombre.ToLower().Contains(filtro.Nombre.ToLower())).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.Direccion))
                {
                    pharmacies = pharmacies.Where(x => x.Direccion.ToLower().Contains(filtro.Direccion.ToLower())).ToList();
                }
            }

            // Paginación
            int totalItems = pharmacies.Count;
            var paginatedPharmacies = pharmacies
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            return Ok(paginatedPharmacies);
        }

        [HttpGet("GetPharmacyById")]
        public async Task<IActionResult> GetPharmacyById(int id)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(x => x.Id == id);

            if (pharmacy == null)
            {
                return NotFound("No se ha encontrado Farmacia");
            }

            return Ok(pharmacy);
        }

        [HttpPost("CreatePharmacy")]
        public async Task<IActionResult> CreatePharmacy(PharmacyDTO pharmacyDTO)
        {
            var pharmacy = new Pharmacy
            {
                Nombre = pharmacyDTO.Nombre,
                Direccion = pharmacyDTO.Direccion
            };

            await _context.Pharmacies.AddAsync(pharmacy);
            await _context.SaveChangesAsync();

            return Ok(pharmacy);
        }

        [HttpPut("UpdatePharmacy")]
        public async Task<IActionResult> UpdatePharmacy(int pharmacyId, [FromBody] PharmacyDTO pharmacyDTO)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(x => x.Id == pharmacyId);

            if (pharmacy == null)
            {
                return NotFound("No se ha encontrado Farmacia");
            }

            pharmacy.Nombre = pharmacyDTO.Nombre;
            pharmacy.Direccion = pharmacyDTO.Direccion;

            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();

            return Ok(pharmacy);
        }

        [HttpDelete("DeletePharmacy")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(x => x.Id == id);

            if (pharmacy == null)
            {
                return NotFound("No se ha encontrado Farmacia");
            }

            _context.Pharmacies.Remove(pharmacy);
            await _context.SaveChangesAsync();

            return Ok("Farmacia eliminda correctamente");
        }

        [HttpPost("ImportPharmaciesExcel")]
        public async Task<IActionResult> ImportPharmacies(IFormFile file)
        {
            if (file?.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest("Debe proporcionar un archivo .xlsx");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var pharmacy = new Pharmacy
                        {
                            Nombre = worksheet.Cells[row, 1].Value.ToString(),
                            Direccion = worksheet.Cells[row, 2].Value.ToString()
                        };

                        await _context.Pharmacies.AddAsync(pharmacy);
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return Ok("Importado Correctamente");
        }
    }
}
