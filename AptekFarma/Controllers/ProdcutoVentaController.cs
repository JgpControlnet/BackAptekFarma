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


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProdcutoVentaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProdcutoVentaController(
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

        [HttpPost("GetAllProducts")]
        public async Task<IActionResult> GetProducts([FromBody] ProductFilterDTO filtro)
        {
            var products = await _context.ProductVenta.ToListAsync();

            if (filtro.Todas)
                return Ok(products);

            if (filtro != null)
            {


                if (!string.IsNullOrEmpty(filtro.Nombre))
                {
                    products = products.Where(x => x.Nombre.ToLower().Contains(filtro.Nombre.ToLower())).ToList();
                }

                if (filtro.PuntosNeceseraios > 0)
                {
                    products = products.Where(x => x.PuntosNeceseraios == filtro.PuntosNeceseraios).ToList();
                }
            }

            // Paginación
            int totalItems = products.Count;
            var paginatedProducts = products
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            return Ok(paginatedProducts);
        }

        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.ProductVenta.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "No se ha encontrado Producto" });
            }

            return Ok(product);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductDTO dto)
        {
            var product = new ProductVenta
            {
                Nombre = dto.Nombre,
                CodProducto = dto.CodProducto,
                Imagen = dto.Imagen,
                PuntosNeceseraios = dto.PuntosNeceseraios,
                CantidadMax = dto.CantidadMax,
                Laboratorio = dto.Laboratorio
            };

            await _context.ProductVenta.AddAsync(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductVenta.ToListAsync();
            return Ok(new { message = "Producto creado correctamente", products });
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductDTO dto)
        {
            var product = await _context.ProductVenta.FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            product.Nombre = dto.Nombre;
            product.Imagen = dto.Imagen;
            product.PuntosNeceseraios = dto.PuntosNeceseraios;

            _context.ProductVenta.Update(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductVenta.ToListAsync();
            return Ok(new { message = "Producto modificado correctamente", products });
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ProductVenta.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "No se ha encontrado Producto" });
            }

            _context.ProductVenta.Remove(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductVenta.ToListAsync();
            return Ok(new { message = "Producto eliminado correctamente", products });
        }

        [HttpPost("AddProductsExcel")]
        public async Task<IActionResult> AddProductsExcel([FromForm] FileDTO dto)
        {
            if (dto.file?.Length == 0 || Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx" });
            }

            var products = new List<ProductVenta>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var stream = new MemoryStream())
            {
                await dto.file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        products.Add(new ProductVenta
                        {
                            CodProducto = int.TryParse(worksheet.Cells[row, 1]?.Text, out int cod) ? cod : 0,
                            Nombre = worksheet.Cells[row, 2]?.Value?.ToString() ?? string.Empty,
                            PuntosNeceseraios = decimal.TryParse(worksheet.Cells[row, 3]?.Text, out decimal precio) ? precio : 0,
                            CantidadMax = int.TryParse(worksheet.Cells[row, 4]?.Text.Split(",")[0], out int cantidad) ? cantidad : 0,
                            Laboratorio = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty
                        });
                    }
                }
            }

            // Guardar en la base de datos
            _context.ProductVenta.AddRange(products);
            await _context.SaveChangesAsync();

            products = await _context.ProductVenta.ToListAsync();

            return Ok(new { message = "Productos importados exitosamente.", products });
        }

    }
}