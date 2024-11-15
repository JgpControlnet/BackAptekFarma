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
using AptekFarma.DTO;


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public SalesController(
            UserManager<User> userManager,
            RoleManager<Roles> roleManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("GetAllSales")]
        public async Task<IActionResult> GetSales([FromBody] SalesFilterDTO filtro)
        {
            var sales = await _context.Sales
                .Include(sale => sale.Campaign)
                .ToListAsync();

            var salesResponse = new List<object>();

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.CodigoNacional))
                {
                    sales = sales.Where(x => x.CodigoNacional == filtro.CodigoNacional).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.Referencia))
                {
                    sales = sales.Where(x => x.Referencia == filtro.Referencia).ToList();
                }

                if (filtro.Nventas != 0)
                {
                    sales = sales.Where(x => x.Nventas == filtro.Nventas).ToList();
                }

                if (filtro.PonderacionPuntos != 0)
                {
                    sales = sales.Where(x => x.PonderacionPuntos >= filtro.PonderacionPuntos).ToList();
                }

                if (filtro.CampaignId != 0)
                {
                    sales = sales.Where(x => x.Campaign.Id == filtro.CampaignId).ToList();
                }
            }

            // Paginación
            int totalItems = sales.Count;
            var paginatedSales = sales
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            foreach (var sale in paginatedSales)
            {
                var response = new
                {
                    sale.Id,
                    sale.CodigoNacional,
                    sale.Referencia,
                    NumeroVentas = sale.Nventas,
                    sale.PonderacionPuntos,
                    Campaign = new
                    {
                        sale.Campaign.Id,
                        sale.Campaign.Nombre,
                        sale.Campaign.Descripcion,
                        sale.Campaign.FechaCaducidad
                    }
                };

                salesResponse.Add(response);
            }


            return Ok(salesResponse);
        }

        [HttpGet("GetSaleById")]
        public async Task<IActionResult> GetSaleById(int id)
        {
            var sale = await _context.Sales
                .Include(sale => sale.Campaign)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound(new { message = "No se ha encontrado la venta" });
            }

            var response = new
            {
                sale.Id,
                sale.CodigoNacional,
                sale.Referencia,
                NumeroVentas = sale.Nventas,
                sale.PonderacionPuntos,
                Campaign = new
                {
                    sale.Campaign.Id,
                    sale.Campaign.Nombre,
                    sale.Campaign.Descripcion,
                    sale.Campaign.FechaCaducidad
                }
            };

            return Ok(response);
        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> CreateSale(SalesDTO sale)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (campaign == null)
            {
                return BadRequest(new { message = "No se encuentra campaña" });
            }

            var newSale = new Sale
            {
                CodigoNacional = sale.CodigoNacional,
                Referencia = sale.Referencia,
                Nventas = sale.Nventas,
                PonderacionPuntos = sale.PonderacionPuntos,
                Campaign = campaign
            };

            _context.Sales.Add(newSale);
            await _context.SaveChangesAsync();

            var response = new
            {
                newSale.Id,
                newSale.CodigoNacional,
                newSale.Referencia,
                NumeroVentas = newSale.Nventas,
                newSale.PonderacionPuntos,
                Campaign = new
                {
                    newSale.Campaign.Id,
                    newSale.Campaign.Nombre,
                    newSale.Campaign.Descripcion,
                    newSale.Campaign.FechaCaducidad
                }
            };

            return Ok(new { message = "Venta creada correctamente" });
        }

        [HttpPut("UpdateSale")]
        public async Task<IActionResult> UpdateSale(int saleId, [FromBody] SalesDTO sale)
        {
            Campaign campaign = null;

            if (sale.CampaignId != 0)
            {
                campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

                if (campaign == null)
                {
                    return BadRequest(new { message = "No se encuentra campaña" });
                }
            }

            var saleToUpdate = await _context.Sales.FirstOrDefaultAsync(x => x.Id == saleId);

            if (saleToUpdate == null)
            {
                return NotFound(new { message = "Venta no encontrada" });
            }

            saleToUpdate.CodigoNacional = sale.CodigoNacional != null ? sale.CodigoNacional : saleToUpdate.CodigoNacional;
            saleToUpdate.Referencia = sale.Referencia != null ? sale.Referencia : saleToUpdate.Referencia;
            saleToUpdate.Nventas = sale.Nventas != 0 ? sale.Nventas : saleToUpdate.Nventas;
            saleToUpdate.PonderacionPuntos = sale.PonderacionPuntos != 0 ? sale.PonderacionPuntos : saleToUpdate.PonderacionPuntos;
            saleToUpdate.Campaign = campaign ?? saleToUpdate.Campaign;

            var response = new
            {
                saleToUpdate.Id,
                saleToUpdate.CodigoNacional,
                saleToUpdate.Referencia,
                NumeroVentas = saleToUpdate.Nventas,
                saleToUpdate.PonderacionPuntos,
                Campaign = new
                {
                    saleToUpdate.Campaign.Id,
                    saleToUpdate.Campaign.Nombre,
                    saleToUpdate.Campaign.Descripcion,
                    saleToUpdate.Campaign.FechaCaducidad
                }
            };

            _context.Update(saleToUpdate);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Venta modificada correctamente" });
        }

        [HttpDelete("DeleteSale")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound(new { message = "No se ha encontrado la venta" });
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Venta borrada correctamente" });
        }

        [HttpPost("AddSalesExcel")]
        public async Task<IActionResult> AddSalesExcel([FromForm] SalesExcelDTO dto)
        {
            if (dto.file?.Length == 0 || Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx" });
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var sales = new List<Sale>();
            using (var package = new ExcelPackage(dto.file.OpenReadStream()))
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
                        return BadRequest(new { message = $"Fecha inválida en la fila {row}" });
                    }

                    Campaign campaign = null;

                    if (dto.newCampaign)
                    {
                        campaign = new Campaign
                        {
                            Nombre = dto.campaignDTO.Nombre,
                            Descripcion = dto.campaignDTO.Descripcion,
                            FechaCaducidad = fechaCaducidad
                        };

                        await _context.Campaigns.AddAsync(campaign);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.FechaCaducidad == fechaCaducidad);

                        if (campaign == null)
                        {
                            return BadRequest(new { message = "No se encuentra campaña" });
                        }
                    }

                    sales.Add(new Sale
                    {
                        CodigoNacional = worksheet.Cells[row, 1].Value.ToString().Trim(),
                        Referencia = worksheet.Cells[row, 2].Value.ToString().Trim(),
                        Nventas = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()),
                        PonderacionPuntos = int.Parse(worksheet.Cells[row, 4].Value.ToString().Trim()),
                        Campaign = campaign
                    });
                }
            }

            await _context.Sales.AddRangeAsync(sales);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Ventas subidas correctamente" });
        }

        

    }
}
