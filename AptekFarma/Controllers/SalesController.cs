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

        [HttpGet("GetAllSales")]
        public async Task<IActionResult> GetSales([FromQuery] SalesFilterDTO filtro)
        {
            var sales = await _context.Sales
                .Include(sale => sale.Product)
                .Include(sale => sale.Seller)
                .Include(sale => sale.Campaign)
                .ToListAsync();

            var salesResponse = new List<object>();

            if (filtro != null)
            {
                if (filtro.ProductoId != 0)
                {
                    sales = sales.Where(x => x.Product.Id == filtro.ProductoId).ToList();
                }

                if (filtro.Cantidad != 0)
                {
                    sales = sales.Where(x => x.Cantidad == filtro.Cantidad).ToList();
                }

                if (filtro.CampaignId != 0)
                {
                    sales = sales.Where(x => x.Campaign.Id == filtro.CampaignId).ToList();
                }

                if (filtro.VendedorId != "")
                {
                    sales = sales.Where(x => x.Seller.Id == filtro.VendedorId).ToList();
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
                    Campaign = new
                    {
                        Id = sale.Campaign.Id,
                        CodigoNacional = sale.Campaign.CodigoNacional,
                        Referencia = sale.Campaign.Referencia,
                        PonderacionPuntos = sale.Campaign.PonderacionPuntos,
                        Nventas = sale.Campaign.Nventas,
                        FechaCaducidad = sale.Campaign.FechaCaducidad
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
            var sale = await _context.Sales
                .Include(sale => sale.Product)
                .Include(sale => sale.Seller)
                .Include(sale => sale.Campaign)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound("No se ha encontrado la venta");
            }

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
                Campaign = new
                {
                    Id = sale.Campaign.Id,
                    CodigoNacional = sale.Campaign.CodigoNacional,
                    Referencia = sale.Campaign.Referencia,
                    PonderacionPuntos = sale.Campaign.PonderacionPuntos,
                    Nventas = sale.Campaign.Nventas,
                    FechaCaducidad = sale.Campaign.FechaCaducidad
                },
                Validated = sale.Validated
            };

            return Ok(response);
        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> CreateSale(SalesDTO sale)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == sale.ProductoId);
            var seller = await _context.Users.FirstOrDefaultAsync(x => x.Id == sale.VendedorId);
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (product == null || seller == null || campaign == null)
            {
                return BadRequest("Invalid product, seller or campaign");
            }

            var newSale = new Sales
            {
                Product = product,
                Cantidad = sale.Cantidad,
                Seller = seller,
                Fecha = DateTime.Now,
                Campaign = campaign,
                Validated = false
            };

            _context.Sales.Add(newSale);
            await _context.SaveChangesAsync();

            var response = new
            {
                Id = newSale.Id,
                Product = new
                {
                    CodigoNacional = newSale.Product.CodigoNacional,
                    Nombre = newSale.Product.Nombre,
                    Imagen = newSale.Product.Imagen,
                    Precio = newSale.Product.Precio
                },
                Cantidad = newSale.Cantidad,
                Seller = new
                {
                    Id = newSale.Seller.Id,
                    UserName = newSale.Seller.UserName,
                    Email = newSale.Seller.Email,
                    Points = newSale.Seller.Points
                },
                Fecha = newSale.Fecha,
                Campaign = new
                {
                    Id = newSale.Campaign.Id,
                    CodigoNacional = newSale.Campaign.CodigoNacional,
                    Referencia = newSale.Campaign.Referencia,
                    PonderacionPuntos = newSale.Campaign.PonderacionPuntos,
                    Nventas = newSale.Campaign.Nventas,
                    FechaCaducidad = newSale.Campaign.FechaCaducidad
                },
                Validated = newSale.Validated
            };

            return Ok(response);
        }

        [HttpPut("UpdateSale")]
        public async Task<IActionResult> UpdateSale(int saleId, [FromBody] SalesDTO sale)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == sale.ProductoId);
            var seller = await _context.Users.FirstOrDefaultAsync(x => x.Id == sale.VendedorId);
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (product == null || seller == null || campaign == null)
            {
                return BadRequest("Invalid product, seller or campaign");
            }

            var saleToUpdate = await _context.Sales.FirstOrDefaultAsync(x => x.Id == saleId);

            if (saleToUpdate == null)
            {
                return NotFound();
            }

            saleToUpdate.Product = product;
            saleToUpdate.Cantidad = sale.Cantidad != 0 ? sale.Cantidad : saleToUpdate.Cantidad;
            saleToUpdate.Seller = seller;
            saleToUpdate.Campaign = campaign;

            var response = new
            {
                Id = saleToUpdate.Id,
                Product = new
                {
                    CodigoNacional = saleToUpdate.Product.CodigoNacional,
                    Nombre = saleToUpdate.Product.Nombre,
                    Imagen = saleToUpdate.Product.Imagen,
                    Precio = saleToUpdate.Product.Precio
                },
                Cantidad = saleToUpdate.Cantidad,
                Seller = new
                {
                    Id = saleToUpdate.Seller.Id,
                    UserName = saleToUpdate.Seller.UserName,
                    Email = saleToUpdate.Seller.Email,
                    Points = saleToUpdate.Seller.Points
                },
                Fecha = saleToUpdate.Fecha,
                Campaign = new
                {
                    Id = saleToUpdate.Campaign.Id,
                    CodigoNacional = saleToUpdate.Campaign.CodigoNacional,
                    Referencia = saleToUpdate.Campaign.Referencia,
                    PonderacionPuntos = saleToUpdate.Campaign.PonderacionPuntos,
                    Nventas = saleToUpdate.Campaign.Nventas,
                    FechaCaducidad = saleToUpdate.Campaign.FechaCaducidad
                },
                Validated = saleToUpdate.Validated
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

        [HttpPut("ValidateSale")]
        public async Task<IActionResult> ValidateSale(int id)
        {
            var sale = await _context.Sales
                .Include(sale => sale.Seller)
                .Include(sale => sale.Campaign)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            if (sale.Validated)
            {
                return BadRequest("Venta ya validada");
            }

            sale.Validated = true;

            var user = sale.Seller;
            var campaign = sale.Campaign;

            user.Points += campaign.PonderacionPuntos * sale.Cantidad;
            campaign.Nventas += 1;

            var pointsEarned = new PointEarned();
            pointsEarned.User = user;
            pointsEarned.Points = campaign.PonderacionPuntos * sale.Cantidad;
            pointsEarned.Fecha = DateTime.Now;

            _context.PointsEarned.Add(pointsEarned);
            _context.Update(sale);
            await _context.SaveChangesAsync();

            return Ok($"Venta {sale.Id} validada correctamente");
        }

    }
}
