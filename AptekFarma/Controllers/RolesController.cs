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
    [Route("/[controller]")]
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
        public async Task<IActionResult> NewRol(Roles rol)
        {
            var roleExist = await _roleManager.RoleExistsAsync(rol.Name);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new Roles { Name = rol.Name });
            }
            return Ok(rol);
        }

        [HttpPut("UpdateRole")]
        public async Task<IActionResult> UpdateRole(Roles rol)
        {
            var roleExist = await _roleManager.RoleExistsAsync(rol.Name);
            if (roleExist)
            {
                await _roleManager.UpdateAsync(new Roles { Name = rol.Name });
            }

            return Ok(rol);
        }

        [HttpDelete("DeleteRole")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var roleExist = await _roleManager.RoleExistsAsync(id);
            if (roleExist)
            {
                await _roleManager.DeleteAsync(new Roles { Id = id });
            }

            return Ok("Rol eliminado correctamente");
        }

    }
}
