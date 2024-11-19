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
using System.Globalization;


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

        [HttpPost("GetAllPharmacies")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPharmacies([FromBody] PharmacyFilterDTO filtro)
        {
            var pharmacies = await _context.Pharmacy
                .ToListAsync();

            if (filtro.Todas)
                return Ok(pharmacies);

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
            var pharmacy = await _context.Pharmacy.FirstOrDefaultAsync(x => x.Id == id);

            if (pharmacy == null)
            {
                return NotFound(new { message = "No se ha encontrado Farmacia" });
            }

            return Ok(pharmacy);
        }

        [HttpPost("CreatePharmacy")]
        public async Task<IActionResult> CreatePharmacy(PharmacyDTO pharmacyDTO)
        {

            var pharmacy = new Pharmacy
            {
                Nombre = pharmacyDTO.Nombre,
                Direccion = pharmacyDTO.Direccion,
                CP = pharmacyDTO.CP,
                Localidad = pharmacyDTO.Localidad,
                Provincia =  pharmacyDTO.Provincia
            };

            await _context.Pharmacy.AddAsync(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacy.ToListAsync();

            return Ok(new { message = "Farmacia creada correctamente", pharmacies });
        }

        [HttpPut("UpdatePharmacy")]
        public async Task<IActionResult> UpdatePharmacy([FromBody] PharmacyDTO pharmacyDTO)
        {
            var pharmacy = await _context.Pharmacy
                .FirstOrDefaultAsync(x => x.Id == pharmacyDTO.id);

          
            if (pharmacy == null)
            {
                return NotFound(new { message = "No se ha encontrado Farmacia" });
            }

            pharmacy.Nombre = pharmacyDTO.Nombre;
            pharmacy.Direccion = pharmacyDTO.Direccion;
            pharmacy.CP = pharmacyDTO.CP;
            pharmacy.Provincia = pharmacyDTO.Provincia;
            pharmacy.Localidad = pharmacyDTO.Localidad;

            _context.Pharmacy.Update(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacy.ToListAsync();

            return Ok(new { message = "Farmacia editada correctamente", pharmacies });
        }

        [HttpDelete("DeletePharmacy")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await _context.Pharmacy.FirstOrDefaultAsync(x => x.Id == id);

            if (pharmacy == null)
            {
                return NotFound(new { message = "No se ha encontrado Farmacia" });
            }

            _context.Pharmacy.Remove(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacy.ToListAsync();
            return Ok(new { message = "Importado Correctamente", pharmacies });
        }

        [HttpPost("ImportPharmaciesExcel")]
        public async Task<IActionResult> ImportPharmacies([FromForm] FileDTO dto)
        {
            if (dto.file?.Length == 0 || (Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".xls" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".csv"))
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx, .xls o .csv" });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<Pharmacy> pharmacies = new List<Pharmacy>();
            List<PharmacyErrorDTO> errors = new List<PharmacyErrorDTO>();

            try {
                using (var stream = new MemoryStream())
                {
                    await dto.file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {

                            var pharmacy = new Pharmacy
                            {
                                Nombre = worksheet.Cells[row, 1]?.Value?.ToString()?.Trim() ?? string.Empty,
                                Direccion = worksheet.Cells[row, 2]?.Value?.ToString()?.Trim() ?? string.Empty,
                                CP = worksheet.Cells[row, 5]?.Value?.ToString()?.Trim() ?? string.Empty,
                                Localidad = worksheet.Cells[row, 3]?.Value?.ToString()?.Trim() ?? string.Empty,
                                Provincia = worksheet.Cells[row, 4]?.Value?.ToString()?.Trim() ?? string.Empty,

                            };

                            //await _context.Pharmacies.AddAsync(pharmacy);

                            pharmacies.Add(pharmacy);
                        }

                        //await _context.SaveChangesAsync();
                    }
                }

                if (errors.Count > 0)
                {
                    return BadRequest(new { message = "Se han encontrado errores", errors });
                }

                await _context.Pharmacy.AddRangeAsync(pharmacies);

                await _context.SaveChangesAsync();

                //importar solo las que su nombre no se vacio o nulo
                pharmacies = pharmacies.Where(x => x.Nombre != null || string.IsNullOrEmpty(x.Nombre)).ToList();
               

                pharmacies = await _context.Pharmacy.ToListAsync();

                return Ok(new { message = "Importado Correctamente", pharmacies });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al importar el archivo", error = ex.Message });
            }

            
        }
    }
}
