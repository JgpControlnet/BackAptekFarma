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
<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
    public class ProductoCampannaController : ControllerBase
========
    public class ProdcutoVentaController : ControllerBase
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
        public ProductoCampannaController(
========
        public ProdcutoVentaController(
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
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
            // Incluye la relación de campaña al obtener los productos
            var query = _context.ProductoCampanna
                .Include(p => p.Campanna)
                .AsQueryable();

            if (filtro.Todas)
                return Ok(await query.ToListAsync());

            if (filtro != null)
            {
                // Filtra por nombre
                if (!string.IsNullOrEmpty(filtro.Nombre))
                {
                    query = query.Where(x => x.Nombre.ToLower().Contains(filtro.Nombre.ToLower()));
                }

<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
                // Filtra por precio
                if (filtro.Precio > 0)
                {
                    query = query.Where(x => x.Puntos == filtro.Precio);
                }

                // Filtra por campaña
                if (filtro.CampannaId > 0)
                {
                    query = query.Where(x => x.CampaignId == filtro.CampannaId);
========
                if (filtro.PuntosNeceseraios > 0)
                {
                    products = products.Where(x => x.PuntosNeceseraios == filtro.PuntosNeceseraios).ToList();
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
                }
            }

            // Paginación
            var paginatedProducts = await query
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToListAsync();

            return Ok(new
            {
               
                Products = paginatedProducts
            });
        }


        [HttpGet("GetProductById")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.ProductoCampanna.Include(x => x.Campanna).FirstOrDefaultAsync(x => x.Id == id);
               
            if (product == null)
            {
                return NotFound(new { message = "No se ha encontrado Producto" });
            }

            return Ok(product);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] ProductoCampannaDTO dto)
        {
            var product = new ProductoCampanna
            {
<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
                Nombre = dto.nombre,
                Codigo = dto.codigo,
                CampaignId = dto.campaignId,
                Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == dto.campaignId),
                Puntos = dto.puntos,
                UnidadesMaximas = dto.unidadesMaximas,
                Laboratorio = dto.laboratorio
========
                Nombre = dto.Nombre,
                CodProducto = dto.CodProducto,
                Imagen = dto.Imagen,
                PuntosNeceseraios = dto.PuntosNeceseraios,
                CantidadMax = dto.CantidadMax,
                Laboratorio = dto.Laboratorio
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
            };

            await _context.ProductoCampanna.AddAsync(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductoCampanna.ToListAsync();
            return Ok(new { message = "Producto creado correctamente", products });
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductoCampannaDTO dto)
        {
            var product = await _context.ProductoCampanna.FirstOrDefaultAsync(x => x.Id == productId);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            product.Nombre = dto.nombre;
            product.Codigo = dto.codigo;
            product.CampaignId = dto.campaignId;
            product.Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == dto.campaignId);
            product.Puntos = dto.puntos;
            product.UnidadesMaximas = dto.unidadesMaximas;
            product.Laboratorio = dto.laboratorio;

          


            _context.ProductoCampanna.Update(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductoCampanna.ToListAsync();
            return Ok(new { message = "Producto modificado correctamente",products });
        }

        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.ProductoCampanna.FirstOrDefaultAsync(x => x.Id == id);

            if (product == null)
            {
                return NotFound(new { message = "No se ha encontrado Producto" });
            }

            _context.ProductoCampanna.Remove(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductoCampanna.ToListAsync();
            return Ok(new { message = "Producto eliminado correctamente", products  });
        }

        [HttpPost("AddProductsExcel")]
        public async Task<IActionResult> AddProductsExcel([FromForm] FileDTO dto, int idCampanna)
        {
            if (dto.file?.Length == 0 || Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx")
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx" });
            }

            var products = new List<ProductoCampanna>();

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
                        products.Add(new ProductoCampanna
                        {
<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
                            Codigo = int.Parse(worksheet.Cells[row, 1].Value.ToString()),
                            Nombre = worksheet.Cells[row, 2].Value.ToString(),
                            CampaignId = idCampanna,
                            Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == idCampanna),
                            Puntos = double.Parse(worksheet.Cells[row, 3].Value.ToString()),
                            UnidadesMaximas = int.Parse(worksheet.Cells[row, 4].Value.ToString()),
                            Laboratorio = worksheet.Cells[row, 5].Value.ToString()
                           
========
                            CodProducto = int.TryParse(worksheet.Cells[row, 1]?.Text, out int cod) ? cod : 0,
                            Nombre = worksheet.Cells[row, 2]?.Value?.ToString() ?? string.Empty,
                            PuntosNeceseraios = decimal.TryParse(worksheet.Cells[row, 3]?.Text, out decimal precio) ? precio : 0,
                            CantidadMax = int.TryParse(worksheet.Cells[row, 4]?.Text.Split(",")[0], out int cantidad) ? cantidad : 0,
                            Laboratorio = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
                        });
                    }
                }
            }

            // Guardar en la base de datos
            _context.ProductoCampanna.AddRange(products);
            await _context.SaveChangesAsync();

<<<<<<<< HEAD:AptekFarma/Controllers/ProdcutoCampannaController.cs
            return Ok(new { message = "Productos campañana importados exitosamente.", products });
========
            products = await _context.ProductVenta.ToListAsync();

            return Ok(new { message = "Productos importados exitosamente.", products });
>>>>>>>> Ventas:AptekFarma/Controllers/ProdcutoVentaController.cs
        }

    }
}
