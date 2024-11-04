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
    public class SalesFormController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public SalesFormController(
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

        [HttpGet("GetAllSalesForm")]
        public async Task<IActionResult> GetSalesForms([FromQuery] SalesFormFilterDTO filtro)
        {
            var salesForms = await _context.SalesForms
                .Include(sale => sale.Product)
                .Include(sale => sale.Seller)
                .Include(sale => sale.Sale)
                .ToListAsync();

            var salesResponse = new List<object>();

            if (filtro != null)
            {
                if(filtro.ProductId != 0)
                {
                    salesForms = salesForms.Where(x => x.Product.Id == filtro.ProductId).ToList();
                }

                if (filtro.Cantidad != 0)
                {
                    salesForms = salesForms.Where(x => x.Cantidad == filtro.Cantidad).ToList();
                }

                if (!string.IsNullOrEmpty(filtro.SellerId))
                {
                    salesForms = salesForms.Where(x => x.Seller.Id == filtro.SellerId).ToList();
                }

                //if (filtro.Fecha != null)
                //{
                //    salesForms = salesForms.Where(x => x.Fecha == filtro.Fecha).ToList();
                //}

                if (filtro.SaleId != 0)
                {
                    salesForms = salesForms.Where(x => x.Sale.Id == filtro.SaleId).ToList();
                }

                if (filtro.Validated)
                {
                    salesForms = salesForms.Where(x => x.Validated == filtro.Validated).ToList();
                }
            }

            // Paginación
            int totalItems = salesForms.Count;
            var paginatedSales = salesForms
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            foreach (var sale in paginatedSales)
            {
                var response = new
                {
                    Id = sale.Id,
                    Product = new
                    {
                        CodigoNacional = sale.Product.CodigoNacional,
                        Nombre = sale.Product.Nombre,
                        Imagen = sale.Product.Imagen,
                        Precio = sale.Product.Precio
                    },
                    Cantidad = sale.Cantidad,
                    Seller = new
                    {
                        Id = sale.Seller.Id,
                        UserName = sale.Seller.UserName,
                        Email = sale.Seller.Email,
                        Points = sale.Seller.Points
                    },
                    Fecha = sale.Fecha,
                    Sale = new
                    {
                        Id = sale.Sale.Id,
                        CodigoNacional = sale.Sale.CodigoNacional,
                        Referencia = sale.Sale.Referencia,
                        PonderacionPuntos = sale.Sale.PonderacionPuntos,
                        Nventas = sale.Sale.Nventas,
                        Campaign = new
                        {
                            Id = sale.Sale.Campaign.Id,
                            Nombre = sale.Sale.Campaign.Nombre,
                            Descripcion = sale.Sale.Campaign.Descripcion,
                            FechaCaducidad = sale.Sale.Campaign.FechaCaducidad
                        }
                    },
                    Validated = sale.Validated
                };

                salesResponse.Add(response);
            }


            return Ok(salesResponse);
        }

        [HttpGet("GetSaleById")]
        public async Task<IActionResult> GetSaleById(int id)
        {
            var saleForm = await _context.SalesForms
                .Include(sale => sale.Product)
                .Include(sale => sale.Seller)
                .Include(sale => sale.Sale)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saleForm == null)
            {
                return NotFound("No se ha encontrado la venta");
            }

            var response = new
            {
                Id = saleForm.Id,
                Product = new
                {
                    CodigoNacional = saleForm.Product.CodigoNacional,
                    Nombre = saleForm.Product.Nombre,
                    Imagen = saleForm.Product.Imagen,
                    Precio = saleForm.Product.Precio
                },
                Cantidad = saleForm.Cantidad,
                Seller = new
                {
                    Id = saleForm.Seller.Id,
                    UserName = saleForm.Seller.UserName,
                    Email = saleForm.Seller.Email,
                    Points = saleForm.Seller.Points
                },
                Fecha = saleForm.Fecha,
                Sale = new
                {
                    Id = saleForm.Sale.Id,
                    CodigoNacional = saleForm.Sale.CodigoNacional,
                    Referencia = saleForm.Sale.Referencia,
                    PonderacionPuntos = saleForm.Sale.PonderacionPuntos,
                    Nventas = saleForm.Sale.Nventas,
                    Campaign = new
                    {
                        Id = saleForm.Sale.Campaign.Id,
                        Nombre = saleForm.Sale.Campaign.Nombre,
                        Descripcion = saleForm.Sale.Campaign.Descripcion,
                        FechaCaducidad = saleForm.Sale.Campaign.FechaCaducidad
                    }
                },
                Validated = saleForm.Validated
            };


            return Ok(response);
        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> CreateSale(SalesDTO sale)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (campaign == null)
            {
                return BadRequest("No se encuentra campaña");
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

            return Ok(response);
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
                    return BadRequest("No se encuentra campaña");
                }
            }

            var saleToUpdate = await _context.Sales.FirstOrDefaultAsync(x => x.Id == saleId);

            if (saleToUpdate == null)
            {
                return NotFound();
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

            return Ok(response);
        }

        [HttpDelete("DeleteSale")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound("No se ha encontrado la venta");
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok("Venta borrada correctamente");
        }

        [HttpPost("AddSalesExcel")]
        public async Task<IActionResult> AddSalesExcel(IFormFile file, bool newCampaign, [FromQuery] CampaignDTO? dto)
        {
            if (file?.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest("Debe proporcionar un archivo .xlsx");
            }

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var sales = new List<Sale>();
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



                    sales.Add(new Sale
                    {
                        CodigoNacional = worksheet.Cells[row, 1].Value.ToString().Trim(),
                        Referencia = worksheet.Cells[row, 2].Value.ToString().Trim(),
                        Nventas = int.Parse(worksheet.Cells[row, 3].Value.ToString().Trim()),
                        PonderacionPuntos = int.Parse(worksheet.Cells[row, 4].Value.ToString().Trim()),
                    });
                }
            }

            await _context.Sales.AddRangeAsync(sales);
            await _context.SaveChangesAsync();
            return Ok(sales);
        }

        [HttpPut("ValidateSale")]
        public async Task<IActionResult> ValidateSale(int id)
        {
            var saleForm = await _context.SalesForms
                .Include(sale => sale.Seller)
                .Include(sale => sale.Sale)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saleForm == null)
            {
                return NotFound();
            }

            if (saleForm.Validated)
            {
                return BadRequest("Venta ya validada");
            }

            saleForm.Validated = true;

            var user = saleForm.Seller;
            var sale = saleForm.Sale;

            user.Points += sale.PonderacionPuntos * saleForm.Cantidad;
            sale.Nventas += 1;

            var pointsEarned = new PointEarned();
            pointsEarned.User = user;
            pointsEarned.Points = sale.PonderacionPuntos * saleForm.Cantidad;
            pointsEarned.Fecha = DateTime.Now;

            _context.PointsEarned.Add(pointsEarned);
            _context.Update(saleForm);
            await _context.SaveChangesAsync();

            return Ok($"Venta {saleForm.Id} validada correctamente");
        }

    }
}
