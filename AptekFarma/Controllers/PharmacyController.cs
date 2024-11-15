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
            var pharmacies = await _context.Pharmacies
                .Include(x => x.Localidad)
                .Include(x => x.Provincia)
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
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(x => x.Id == id);

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
                Localidad = _context.Localidades.FirstOrDefault(x => x.Nombre == pharmacyDTO.Localidad),
                Provincia = _context.Provincias.FirstOrDefault(x => x.Nombre == pharmacyDTO.Provincia)
            };

            await _context.Pharmacies.AddAsync(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacies.ToListAsync();

            return Ok(new { message = "Farmacia creada correctamente", pharmacies });
        }

        [HttpPut("UpdatePharmacy")]
        public async Task<IActionResult> UpdatePharmacy(int pharmacyId, [FromBody] PharmacyDTO pharmacyDTO)
        {
            var pharmacy = await _context.Pharmacies
                .Include(x => x.Localidad)
                .Include(x => x.Provincia)
                .FirstOrDefaultAsync(x => x.Id == pharmacyId);

            var localidad = _context.Localidades.FirstOrDefault(x => x.Nombre == pharmacyDTO.Localidad);
            var provincia = _context.Provincias.FirstOrDefault(x => x.Nombre == pharmacyDTO.Provincia);

            if (pharmacy == null)
            {
                return NotFound(new { message = "No se ha encontrado Farmacia" });
            }

            pharmacy.Nombre = pharmacyDTO.Nombre;
            pharmacy.Direccion = pharmacyDTO.Direccion;
            pharmacy.CP = pharmacyDTO.CP;
            pharmacy.Provincia = provincia;
            pharmacy.Localidad = localidad;

            _context.Pharmacies.Update(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacies.ToListAsync();

            return Ok(new { message = "Farmacia editada correctamente", pharmacies });
        }

        [HttpDelete("DeletePharmacy")]
        public async Task<IActionResult> DeletePharmacy(int id)
        {
            var pharmacy = await _context.Pharmacies.FirstOrDefaultAsync(x => x.Id == id);

            if (pharmacy == null)
            {
                return NotFound(new { message = "No se ha encontrado Farmacia" });
            }

            _context.Pharmacies.Remove(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacies.ToListAsync();
            return Ok(new { message = "Importado Correctamente", pharmacies });
        }

        [HttpPost("ImportPharmaciesExcel")]
        public async Task<IActionResult> ImportPharmacies([FromForm] FileDTO dto)
        {
            if (dto.file?.Length == 0 || Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx" });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            List<Pharmacy> pharmacies = new List<Pharmacy>();
            List<PharmacyErrorDTO> errors = new List<PharmacyErrorDTO>();

            using (var stream = new MemoryStream())
            {
                await dto.file.CopyToAsync(stream);

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                   

                    for (int row = 2; row <= rowCount; row++)
                    {
                        Localidad localidad = null;
                        bool localidadIncorrecta = false;
                        bool provinciaIncorrecta = false;
                        if (worksheet.Cells[row, 4].Value != null)
                        {
                            localidad = _context.Localidades
                                .AsEnumerable()
                                .FirstOrDefault(x => NormalizeString(x.Nombre) == NormalizeString(worksheet.Cells[row, 4].Value.ToString()));
                            if (localidad == null)
                            {
                                localidadIncorrecta = true;
                            }

                        }

                        Provincia provincia = null;
                        if (worksheet.Cells[row, 5].Value != null)
                        {
                            provincia = _context.Provincias
                                .AsEnumerable()
                                .FirstOrDefault(x => NormalizeString(x.Nombre) == NormalizeString(worksheet.Cells[row, 5].Value.ToString()));
                            if (provincia == null)
                            {
                                provinciaIncorrecta = true;
                            }
                        }

                        // Si no existe la población o la provincia, no se importa la farmacia
                        if (localidadIncorrecta || provinciaIncorrecta)
                        {
                            List<string> errores = new List<string>();
                            if (localidadIncorrecta)
                            {
                                errores.Add("No existe la localidad");
                            }
                            if (provinciaIncorrecta)
                            {
                                errores.Add("No existe la provincia");
                            }
                            // Se guarda el error
                            errors.Add(new PharmacyErrorDTO
                            {
                                Nombre = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty,
                                Direccion = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                                CP = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty,
                                Localidad = worksheet.Cells[row, 4].Value?.ToString() ?? string.Empty,
                                Provincia = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty,
                                Linea = row,
                                Errores = errores
                            });

                            continue;
                        }

                        var pharmacy = new Pharmacy
                        {
                            Nombre = worksheet.Cells[row, 1].Value?.ToString() ?? string.Empty,
                            Direccion = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                            CP = worksheet.Cells[row, 3].Value?.ToString() ?? string.Empty,
                            LocalidadID = localidad?.Id,
                            ProvinciaID = provincia?.Id
                        };

                        //await _context.Pharmacies.AddAsync(pharmacy);

                        pharmacies.Add(pharmacy);
                    }

                    //await _context.SaveChangesAsync();
                }
            }

            //si existe algun error no importa ninguna farmacia
            if (errors.Count > 0)
            {
                return BadRequest(new { message = "Se han encontrado errores", errors });
            }

            await _context.Pharmacies.AddRangeAsync(pharmacies);

            await _context.SaveChangesAsync();

            pharmacies = await _context.Pharmacies.ToListAsync();

            return Ok(new { message = "Importado Correctamente", pharmacies });
        }

        string NormalizeString(string input)
        {
            if (input == null) return null;

            return string.Concat(input
                .Trim() 
                .ToLowerInvariant() // Convierte a minúsculas 
                .Normalize(NormalizationForm.FormD) // Normaliza la cadena para dividir caracteres con tildes en base + tilde, á en a + ´
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)); // Remueve los caracteres de marca de tildes
        }
    }
}
