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


namespace _AptekFarma.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserController(
            UserManager<User> userManager,
            RoleManager<Roles> roleManager,
            SignInManager<User> signInManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
        }


        [HttpPost("registro")]
        public async Task<IActionResult> registro([FromBody] RegisterDTO dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                nombre = dto.Nombre,
                apellidos = dto.Apellidos,
                nif = dto.Nif,
                fecha_nacimiento = dto.FechaNacimiento.ToString(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
            };

            await _userManager.CreateAsync(user, dto.Password);
            await _userManager.AddToRoleAsync(user, dto.rol);

            await _context.SaveChangesAsync();

            return Ok(new { Token = GenerateJwtToken(user) });

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            // Try to find user by email
            var user = await _userManager.FindByEmailAsync(model.Username);

            // If not found by email, try to find by username
            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
            }

            // If user is found
            if (user != null)
            {
                // Attempt to sign in the user
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {

                    var Token = GenerateJwtToken(user);
                    var rol = await _userManager.GetRolesAsync(user);
                    return Ok(new { Token.Result, rol, user.Id });

                }




                return BadRequest(new { success = false, error = "La solicitud no fue exitosa." });
            }

            // Return failed login response
            return BadRequest(new { success = false, error = "La solicitud no fue exitosa." });
        }
     


        // GET: api/Usuarios
        [HttpGet("ListUsuario")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsuarios()
        {
            var users = await _context.Users.ToListAsync();
            List<UserDTO> result = new List<UserDTO>();
            foreach (var item in users)
            {
                var user = new UserDTO();
                user.Id = item.Id;
                user.UserName = item.UserName;
                user.Email = item.Email;
                user.Nif = item.nif;
                user.Nombre = item.nombre;
                user.Apellidos = item.apellidos;
                user.PhoneNumber = item.PhoneNumber;
                user.FechaNacimiento = item.fecha_nacimiento;
                user.rol = _userManager.GetRolesAsync(item).Result.FirstOrDefault();
                result.Add(user);
            }

            return result;
        }

        // GET: api/Usuarios/5
        [HttpGet("Usuario")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUsuario(string id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                return NotFound();
            }

            var user = new UserDTO();
            user.Id = usuario.Id;
            user.UserName = usuario.UserName;
            user.Email = usuario.Email;
            user.Nif = usuario.nif;
            user.Nombre = usuario.nombre;
            user.Apellidos = usuario.apellidos;
            user.PhoneNumber = usuario.PhoneNumber;
            user.FechaNacimiento = usuario.fecha_nacimiento;
            user.rol = _userManager.GetRolesAsync(usuario).Result.FirstOrDefault();

            return user;
        }

        // PUT: api/Usuarios/5
        [HttpPut("ModificarUsuario")]
        [Authorize]
        public async Task<IActionResult> PutUsuario(UserDTO dto)
        {
            if (dto.Id == "")
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);

            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }

            user.UserName = dto.UserName != "" && dto.UserName != "string" ? dto.UserName : user.UserName;
            user.Email = dto.Email != "" && dto.Email != "string" ? dto.Email : user.Email;
            user.PhoneNumber = dto.PhoneNumber != "" && dto.PhoneNumber != "string" ? dto.PhoneNumber : user.PhoneNumber;
            user.nombre = dto.Nombre != "" && dto.Nombre != "string" ? dto.Nombre : user.nombre;
            user.apellidos = dto.Apellidos != "" && dto.Apellidos != "string" ? dto.Apellidos : user.apellidos;
            user.nif = dto.Nif != "" && dto.Nif != "string" ? dto.Nif : user.nif;
            user.fecha_nacimiento = dto.FechaNacimiento != "" && dto.FechaNacimiento != "string" ? dto.FechaNacimiento : user.fecha_nacimiento;

            _context.Users.Update(user);

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
                return NotFound();
                
            }

            return Ok();
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("EliminarUsuario")]
        [Authorize]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            _context.Users.Remove(usuario);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("CambiarRol")]
        [Authorize]
        public async Task<IActionResult> CambiarRol(string id, string rol)
        {
            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, roles);
            await _userManager.AddToRoleAsync(usuario, rol);

            return Ok();
        }

        private bool UsuarioExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }


        private async Task<string> GenerateJwtToken(IdentityUser user)
        {
            var claims = new[]
                    {
                        new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }


    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class TokenResponse
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
    }

    public class UserGoogle
    {
        public string uid { get; set; }
        public string email { get; set; }
        public string displayName { get; set; }
        public string? photoUrl { get; set; }
        public string? phoneNumber { get; set; }
        public bool isAnonymous { get; set; }
        public bool isEmailVerified { get; set; }
        public string? providerId { get; set; }
        public string? tenantId { get; set; }
        public string? refreshToken { get; set; }
        public long creationTimestamp { get; set; }
        public long lastSignInTimestamp { get; set; }
    }
}
