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

        [HttpGet("GetSalesFormById")]
        public async Task<IActionResult> GetSalesFormById(int id)
        {
            var saleForm = await _context.SalesForms
                .Include(sale => sale.Product)
                .Include(sale => sale.Seller)
                .Include(sale => sale.Sale)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saleForm == null)
            {
                return NotFound(new { message = "No se ha encontrado la venta" });
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

        [HttpPost("CreateSalesForm")]
        public async Task<IActionResult> CreateSalesForm(SalesFormFilterDTO saleForm)
        {
            var sale = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == saleForm.SaleId);

            if (sale == null)
            {
                return BadRequest(new { message = "No se encuentra venta" });
            }

            var newSaleForm = new SaleForm
            {
                ProductID = saleForm.ProductId,
                Cantidad = saleForm.Cantidad,
                SellerID = saleForm.SellerId,
                Fecha = saleForm.Fecha,
                SaleID = saleForm.SaleId,
                Validated = saleForm.Validated
            };

            _context.SalesForms.Add(newSaleForm);
            await _context.SaveChangesAsync();

            var response = new
            {
                Id = sale.Id,
                Product = new
                {
                    CodigoNacional = newSaleForm.Product.CodigoNacional,
                    Nombre = newSaleForm.Product.Nombre,
                    Imagen = newSaleForm.Product.Imagen,
                    Precio = newSaleForm.Product.Precio
                },
                Cantidad = newSaleForm.Cantidad,
                Seller = new
                {
                    Id = newSaleForm.Seller.Id,
                    UserName = newSaleForm.Seller.UserName,
                    Email = newSaleForm.Seller.Email,
                    Points = newSaleForm.Seller.Points
                },
                Fecha = newSaleForm.Fecha,
                Sale = new
                {
                    Id = newSaleForm.Sale.Id,
                    CodigoNacional = newSaleForm.Sale.CodigoNacional,
                    Referencia = newSaleForm.Sale.Referencia,
                    PonderacionPuntos = newSaleForm.Sale.PonderacionPuntos,
                    Nventas = newSaleForm.Sale.Nventas,
                    Campaign = new
                    {
                        Id = newSaleForm.Sale.Campaign.Id,
                        Nombre = newSaleForm.Sale.Campaign.Nombre,
                        Descripcion = newSaleForm.Sale.Campaign.Descripcion,
                        FechaCaducidad = newSaleForm.Sale.Campaign.FechaCaducidad
                    }
                },
                Validated = newSaleForm.Validated
            };


            return Ok(new { message = "Formulariio de venta creado correctamente" });
        }

        [HttpPut("UpdateSalesForm")]
        public async Task<IActionResult> UpdateSalesForm([FromBody] SalesFormFilterDTO saleForm)
        {
            var sale = await _context.SalesForms.FirstOrDefaultAsync(x => x.Id == saleForm.saleFormId);
            if (sale == null) {
                return NotFound(new { message = "No se ha encontrado la venta" });
            }

            var response = await _context.SalesForms.
                Include(sale => sale.Product).
                Include(sale => sale.Seller).
                Include(sale => sale.Sale).
                FirstOrDefaultAsync(x => x.Id == saleForm.saleFormId);

            if (response == null)
            {
                return NotFound(new { message = "No se ha encontrado la venta" });
            }

            sale.ProductID = saleForm.ProductId;
            sale.Cantidad = saleForm.Cantidad;
            sale.SellerID = saleForm.SellerId;
            sale.Fecha = saleForm.Fecha;
            sale.SaleID = saleForm.SaleId;
            sale.Validated = saleForm.Validated;

            _context.Update(response);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Formulario de venta editado correctamente" });
        }

        [HttpDelete("DeleteSalesForm")]
        public async Task<IActionResult> DeleteSalesForm(int id)
        {
            var saleForm = await _context.SalesForms.FirstOrDefaultAsync(x => x.Id == id);

            if (saleForm == null)
            {
                return NotFound(new { message = "No se ha encontrado la venta" });
            }

            _context.SalesForms.Remove(saleForm);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Formulario de venta borrada correctamente" });
        }

        [HttpPut("ValidateSalesForm")]
        public async Task<IActionResult> ValidateSalesForm(int id)
        {
            var saleForm = await _context.SalesForms
                .Include(sale => sale.Seller)
                .Include(sale => sale.Sale)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (saleForm == null)
            {
                return NotFound(new { message = "Formulario de venta no encontrado" });
            }

            if (saleForm.Validated)
            {
                return BadRequest(new { message = "Formulario de venta ya validada" });
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

            return Ok(new { message = "Venta validada correctamente" });
        }

    }
}
