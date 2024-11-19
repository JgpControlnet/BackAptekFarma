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
using Humanizer;


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoCampannaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductoCampannaController(
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
                if (!string.IsNullOrEmpty(filtro.nombre))
                {
                    query = query.Where(x => x.Nombre.ToLower().Contains(filtro.nombre.ToLower()));
                }

                // Filtra por precio
                if (filtro.precio > 0)
                {
                    query = query.Where(x => x.Puntos == filtro.precio);
                }

                // Filtra por campaña
                if (filtro.campannaId > 0)
                {
                    query = query.Where(x => x.CampannaId == filtro.campannaId);
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
                Nombre = dto.nombre,
                Codigo = dto.codigo,
                CampannaId = dto.campannaId,
                Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == dto.campannaId),
                Puntos = dto.puntos,
                UnidadesMaximas = dto.unidadesMaximas,
                Laboratorio = dto.laboratorio
            };

            await _context.ProductoCampanna.AddAsync(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductoCampanna.Where(x => x.CampannaId == product.CampannaId).ToListAsync();
            
            return Ok(new { message = "Producto creado correctamente", products });
        }

        [HttpPut("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductoCampannaDTO dto)
        {
            var product = await _context.ProductoCampanna.FirstOrDefaultAsync(x => x.Id == dto.id);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            product.Nombre = dto.nombre;
            product.Codigo = dto.codigo;
            product.CampannaId = dto.campannaId;
            product.Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == dto.campannaId);
            product.Puntos = dto.puntos;
            product.UnidadesMaximas = dto.unidadesMaximas;
            product.Laboratorio = dto.laboratorio;




            _context.ProductoCampanna.Update(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductoCampanna.Where(x => x.CampannaId == product.CampannaId).ToListAsync();
           
            return Ok(new { message = "Producto modificado correctamente", products });
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
            var products = await _context.ProductoCampanna.Where(x => x.CampannaId == product.CampannaId).ToListAsync();
            
            return Ok(new { message = "Producto eliminado correctamente", products });
        }

        [HttpPost("AddProductsExcel")]
        public async Task<IActionResult> AddProductsExcel([FromForm] FileDTO dto, int idCampanna)
        {
            if (dto.file?.Length == 0 || (Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".xls" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".csv"))
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx, .xls o .csv" });
            }

            var products = new List<ProductoCampanna>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try {
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
                                Codigo = worksheet.Cells[row, 1].Value != null ? int.Parse(worksheet.Cells[row, 1].Value.ToString()) : 0,
                                Nombre = worksheet.Cells[row, 2].Value?.ToString() ?? string.Empty,
                                CampannaId = idCampanna,
                                Campanna = await _context.Campanna.FirstOrDefaultAsync(x => x.Id == idCampanna),
                                Puntos = worksheet.Cells[row, 3].Value != null ? double.Parse(worksheet.Cells[row, 3].Value.ToString()) : 0,
                                UnidadesMaximas = worksheet.Cells[row, 4].Value != null ? int.Parse(worksheet.Cells[row, 4].Value.ToString()) : 0,
                                Laboratorio = worksheet.Cells[row, 5].Value?.ToString() ?? string.Empty
                            });
                        }
                    }
                }


                products = products.Where(x => x.Codigo != 0).ToList();
                // Guardar en la base de datos
                _context.ProductoCampanna.AddRange(products);
                await _context.SaveChangesAsync();

                var productsCamapanna = await _context.ProductoCampanna.Where(x => x.CampannaId == idCampanna).ToListAsync();
                products = productsCamapanna;
                return Ok(new { message = "Productos campañana importados exitosamente.", products });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al importar el archivo", error = ex.Message });
            }
        }
    }
}