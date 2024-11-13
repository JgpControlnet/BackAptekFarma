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
    public class RolesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public RolesController(
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

        [HttpGet("GetAllRoles")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }

        [HttpGet("GetRoleById")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(role);
        }

        [HttpPost("AddRole")]
        public async Task<IActionResult> NewRol(RoleDTO rol)
        {
            var roleExist = await _roleManager.RoleExistsAsync(rol.Name);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new Roles { Name = rol.Name, Descripcion = rol.Descripcion });
            }
            return Ok(new { message = "Rol creado correctamente" });
        }

        [HttpPut("UpdateRole")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] RoleDTO dto)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == roleId);
            if (role == null)
            {
                return NotFound("Rol no encontrado");
            }

            role.Name = string.IsNullOrWhiteSpace(dto.Name) ? role.Name : dto.Name;
            role.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? role.Descripcion : dto.Descripcion;

            await _roleManager.UpdateAsync(role);
            return Ok(new { message = "Rol modificado correctamente" });
        }

        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = _context.Roles.FirstOrDefault(x => x.Id == id);

            if (role == null)
            {
                return NotFound(new { message = "Rol no encontrado" });
            }

            await _roleManager.DeleteAsync(role);
            return Ok(new { message = "Rol eliminado correctamente" });
        }

    }
}
