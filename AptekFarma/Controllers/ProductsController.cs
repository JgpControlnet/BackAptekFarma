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
    public class ProductsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductsController(
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

        [HttpGet("GetAllProducts")]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound("No se ha encontrado Producto");
            }

            return Ok(product);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDTO dto)
        {
            var product = new Products
            {
                CodigoNacional = dto.CodigoNacional,
                Nombre = dto.Nombre,
                Imagen = dto.Imagen,
                Precio = dto.Precio
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDTO dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return NotFound("No se ha encontrado Producto");
            }

            product.CodigoNacional = dto.CodigoNacional;
            product.Nombre = dto.Nombre;
            product.Imagen = dto.Imagen;
            product.Precio = dto.Precio;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound("No se ha encontrado Producto");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return Ok("Producto eliminado correctamente");
        }

        [HttpPost("AddProductsExcel")]
        public async Task<IActionResult> AddProductsExcel(IFormFile file)
        {
            if (file?.Length == 0 || Path.GetExtension(file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest("Debe proporcionar un archivo .xlsx");
            }

            var products = new List<Products>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        products.Add(new Products
                        {
                            CodigoNacional = worksheet.Cells[row, 1]?.Text?.Trim(),
                            Nombre = worksheet.Cells[row, 2]?.Text?.Trim(),
                            Imagen = worksheet.Cells[row, 3]?.Text?.Trim(),
                            Precio = int.TryParse(worksheet.Cells[row, 4]?.Text, out int precio) ? precio : 0
                        });
                    }
                }
            }

            // Guardar en la base de datos
            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();

            return Ok("Productos importados exitosamente.");
        }

    }
}
