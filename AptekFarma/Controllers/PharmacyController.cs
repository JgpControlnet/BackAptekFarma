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
    [Route("rest/[controller]")]
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
                .Where(x => x.Activo == true)
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
            pharmacies = pharmacies.OrderBy(x => x.Id).ToList();
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
            var pharmacy = await _context.Pharmacy.FirstOrDefaultAsync(x => x.Id == id && x.Activo == true);

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
            //traer todas las farmacias ordenadas descendentemente
            var pharmacies = await _context.Pharmacy.Where(p => p.Activo == true).ToListAsync();
            pharmacies = pharmacies.OrderByDescending(x => x.Id).ToList();

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
            var pharmacies = await _context.Pharmacy.Where(p => p.Activo == true).ToListAsync();
            pharmacies = pharmacies.OrderByDescending(x => x.Id).ToList();
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

            pharmacy.Activo = false;
            _context.Pharmacy.Update(pharmacy);
            await _context.SaveChangesAsync();
            var pharmacies = await _context.Pharmacy.Where(p => p.Activo == true).ToListAsync();
            pharmacies = pharmacies.OrderByDescending(x => x.Id).ToList();
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

            try
            {
                using (var stream = new MemoryStream())
                {
                    await dto.file.CopyToAsync(stream);
                    stream.Position = 0; // Asegúrate de reiniciar la posición del stream

                    using (var package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                        int rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var nombre = worksheet.Cells[row, 1]?.Value?.ToString()?.Trim() ?? string.Empty;
                            var direccion = worksheet.Cells[row, 2]?.Value?.ToString()?.Trim() ?? string.Empty;
                            var localidad = worksheet.Cells[row, 3]?.Value?.ToString()?.Trim() ?? string.Empty;
                            var provincia = worksheet.Cells[row, 4]?.Value?.ToString()?.Trim() ?? string.Empty;
                            var cp = worksheet.Cells[row, 5]?.Value?.ToString()?.Trim() ?? string.Empty;

                            // Verificar si la farmacia ya existe en la base de datos
                            var existingPharmacy = await _context.Pharmacy.FirstOrDefaultAsync(x => x.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));

                            if (existingPharmacy != null)
                            {
                                // Actualizar la farmacia existente
                                existingPharmacy.Direccion = direccion;
                                existingPharmacy.Localidad = localidad;
                                existingPharmacy.Provincia = provincia;
                                existingPharmacy.CP = cp;
                                existingPharmacy.Activo = true;
                            }
                            else
                            {
                                // Agregar una nueva farmacia
                                pharmacies.Add(new Pharmacy
                                {
                                    Nombre = nombre,
                                    Direccion = direccion,
                                    Localidad = localidad,
                                    Provincia = provincia,
                                    CP = cp,
                                    Activo = true
                                });
                            }
                        }
                    }
                }
                pharmacies = pharmacies.Where(x => x.Nombre != null || string.IsNullOrEmpty(x.Nombre)).ToList();
                // Guardar los nuevos registros en la base de datos
                if (pharmacies.Count > 0)
                {
                    await _context.Pharmacy.AddRangeAsync(pharmacies);
                }

                await _context.SaveChangesAsync();

                // Obtener todas las farmacias después de la operación
                pharmacies = await _context.Pharmacy.Where(p => p.Activo == true).ToListAsync();
                pharmacies = pharmacies.OrderByDescending(x => x.Id).ToList();

                return Ok(new { message = "Importado Correctamente", pharmacies });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al importar el archivo", error = ex.Message });
            }
        }
    }
}
