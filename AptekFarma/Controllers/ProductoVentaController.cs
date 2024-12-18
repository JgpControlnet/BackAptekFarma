using AptekFarma.Models;
using AptekFarma.DTO;
using AptekFarma.Context;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;


namespace AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoVentaController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public ProductoVentaController(
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
        public async Task<IActionResult> GetProducts([FromBody] ProductVentaFilterDTO filtro)
        {
            var products = await _context.ProductVenta
                .Where(x => x.Activo == true)
                .ToListAsync();

            if (filtro.Todas)
                return Ok(products);

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.nombre))
                {
                    products = products.Where(x => x.Nombre.ToLower().Contains(filtro.nombre.ToLower())).ToList();
                }
                if (filtro.puntosDesde.HasValue)
                {
                    products = products.Where(x => x.PuntosNecesarios >= filtro.puntosDesde.Value).ToList();
                }

                if (filtro.puntosHasta.HasValue)
                {
                    products = products.Where(x => x.PuntosNecesarios <= filtro.puntosHasta.Value).ToList();
                }
            }

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
            var product = await _context.ProductVenta.FirstOrDefaultAsync(x => x.Id == id && x.Activo == true);

            if (product == null)
            {
                return NotFound(new { message = "No se ha encontrado Producto" });
            }
            return Ok(product);
        }

        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProduct([FromBody] DTO.ProductoVentaDTO dto)
        {
            var product = new AptekFarma.Models.ProductoVenta
            {
                Nombre = dto.nombre,
                CodProducto = dto.codProducto,
                Imagen = dto.imagen,
                Descripcion = dto.descripcion,
                PuntosNecesarios = dto.puntosNecesarios,
                CantidadMax = dto.cantidadMax,
                Laboratorio = dto.laboratorio
            };

            await _context.ProductVenta.AddAsync(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductVenta.ToListAsync();
            return Ok(new { message = "Producto creado correctamente", products });
        }

        [HttpPut("UpdateProduct")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateProduct([FromBody] DTO.ProductoVentaDTO dto)
        {
            var product = await _context.ProductVenta.FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado" });
            }

            product.Nombre = dto.nombre;
            product.Imagen = dto.imagen;
            product.Descripcion = dto.descripcion;
            product.PuntosNecesarios = dto.puntosNecesarios;
            product.CantidadMax = dto.cantidadMax;
            product.Laboratorio = dto.laboratorio;
            product.CodProducto = dto.codProducto;

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

            product.Activo = false;
            _context.ProductVenta.Update(product);
            await _context.SaveChangesAsync();
            var products = await _context.ProductVenta.ToListAsync();
            return Ok(new { message = "Producto eliminado correctamente", products });
        }

        [HttpPost("AddProductsExcel")]
        public async Task<IActionResult> AddProductsExcel([FromForm] FileDTO dto)
        {
            if (dto.file?.Length == 0 || (Path.GetExtension(dto.file.FileName)?.ToLower() != ".xlsx" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".xls" && Path.GetExtension(dto.file.FileName)?.ToLower() != ".csv"))
            {
                return BadRequest(new { message = "Debe proporcionar un archivo .xlsx, .xls o .csv" });
            }

            var products = new List<AptekFarma.Models.ProductoVenta>();

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
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
                            var codProducto = int.TryParse(worksheet.Cells[row, 1]?.Text, out int cod) ? cod : 0;
                            var nombre = worksheet.Cells[row, 2]?.Value?.ToString() ?? string.Empty;
                            var puntosNecesarios = decimal.TryParse(worksheet.Cells[row, 3]?.Text, out decimal precio) ? precio : 0;
                            var cantidadString = worksheet.Cells[row, 4]?.Text;
                            var cantidadMax = decimal.TryParse(cantidadString, out decimal cantidadDec) ? (int)cantidadDec : 0;
                            var laboratorio = worksheet.Cells[row, 5]?.Value?.ToString() ?? string.Empty;

                            // Verificar si el producto ya existe en la base de datos
                            var existingProduct = await _context.ProductVenta.FirstOrDefaultAsync(x => x.CodProducto == codProducto);

                            if (existingProduct != null)
                            {
                                // Actualizar el producto existente
                                existingProduct.Nombre = nombre;
                                existingProduct.PuntosNecesarios = puntosNecesarios;
                                existingProduct.CantidadMax = cantidadMax;
                                existingProduct.Laboratorio = laboratorio;
                                existingProduct.Activo = true;
                            }
                            else
                            {
                                // Agregar un nuevo producto
                                products.Add(new AptekFarma.Models.ProductoVenta
                                {
                                    CodProducto = codProducto,
                                    Nombre = nombre,
                                    PuntosNecesarios = puntosNecesarios,
                                    CantidadMax = cantidadMax,
                                    Laboratorio = laboratorio,
                                    Activo = true
                                });
                            }
                        }
                    }
                }
                products = products.Where(x => x.CodProducto != 0).ToList();
                if (products.Count > 0)
                {
                    _context.ProductVenta.AddRange(products);
                }
                await _context.SaveChangesAsync();

                // Obtener todos los productos después de la operación
                var allProducts = await _context.ProductVenta.ToListAsync();

                return Ok(new { message = "Productos importados exitosamente.", products = allProducts });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al importar el archivo", error = ex.Message });
            }
        }
    }
}