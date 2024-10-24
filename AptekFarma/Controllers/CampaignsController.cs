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
    public class CampaignsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CampaignsController(
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

        [HttpGet("GetAllCampaigns")]
        public async Task<IActionResult> GetCampaigns()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            return Ok(campaigns);
        }

        [HttpGet("GetCampaignById")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(campaign);
        }

        [HttpPost("AddCampaign")]
        public async Task<IActionResult> AddCampaign(Campaigns campaign)
        {
            await _context.Campaigns.AddAsync(campaign);
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpPut("UpdateCampaign")]
        public async Task<IActionResult> UpdateCampaign(Campaigns campaign)
        {
            _context.Campaigns.Update(campaign);
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpDelete("DeleteCampaign")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id);
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpPost("AddCampaignsExcel")]
        public async Task<IActionResult> AddCampaignsExcel(IFormFile file)
        {
            if (file?.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest("Debe proporcionar un archivo .xlsx");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var campaigns = new List<Campaigns>();
            using (var package = new ExcelPackage(file.OpenReadStream()))
            {
                var worksheet = package.Workbook.Worksheets[0];
                var rowCount = worksheet.Dimension.Rows;

                for (int row = 2; row <= rowCount; row++)
                {
                    var fechaCaducidadValue = worksheet.Cells[row, 5].Value?.ToString().Trim();
                    DateTime fechaCaducidad;

                    // Intentar convertir directamente como DateTime
                    if (DateTime.TryParse(fechaCaducidadValue, out fechaCaducidad))
                    {
                        // Conversion exitosa, usar la fecha
                    }
                    else if (double.TryParse(fechaCaducidadValue, out double fechaExcel))
                    {
                        // Si es un número, interpretarlo como fecha en formato Excel
                        fechaCaducidad = DateTime.FromOADate(fechaExcel);
                    }
                    else
                    {
                        // Manejar el error si no se puede convertir
                        return BadRequest($"Fecha inválida en la fila {row}");
                    }

                    campaigns.Add(new Campaigns
                    {
                        CodigoNacional = worksheet.Cells[row, 1].Value.ToString().Trim(),
                        Referencia = worksheet.Cells[row, 2].Value.ToString().Trim(),
                        Nventas = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()),
                        PonderacionPuntos = int.Parse(worksheet.Cells[row, 4].Value.ToString().Trim()),
                        FechaCaducidad = fechaCaducidad
                    });
                }
            }

            await _context.Campaigns.AddRangeAsync(campaigns);
            await _context.SaveChangesAsync();
            return Ok(campaigns);
        }


    }
}
