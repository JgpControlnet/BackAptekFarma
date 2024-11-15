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
using System.Text.RegularExpressions;
using Humanizer;
using AptekFarma.Models;


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
            var validation = ValidationsUserRegister(dto);
            if (validation != "ok")
            {
                return BadRequest(new { message = validation });
            }

            var user = new User
            {
                UserName = dto.UserName,
                nombre = dto.Nombre,
                apellidos = dto.Apellidos,
                nif = dto.Nif,
                fecha_nacimiento = dto.FechaNacimiento.ToString(),
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Pharmacy = _context.Pharmacy.FirstOrDefault(p => p.Id == dto.PharmacyId)

            };
            
            await _userManager.CreateAsync(user, dto.Password);
            await _userManager.AddToRoleAsync(user, "Farma");

            return Ok(new { message = "Usuario creado correctamente" });

        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username);

            if (user == null)
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == model.Username);
            }

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {

                    var Token = await GenerateJwtAndRefreshToken(user);
                    var rol = await _userManager.GetRolesAsync(user);
                    user.RememberMe = model.RemembeMe;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return Ok(new { Token, rol = rol[0], user.Id, nombre=user.nombre+" "+user.apellidos, user.Points });

                }

                return BadRequest(new { success = false, error = "La solicitud no fue exitosa." });
            }

            // Return failed login response
            return BadRequest(new { success = false, error = "La solicitud no fue exitosa." });
        }

        // POST: api/Usuarios
        [HttpPost("ListUsuario")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsuarios([FromBody] UserFilterDTO filtro)
        {
            var users = await _context.Users.Include(x => x.Pharmacy).ToListAsync();
            List<UserDTO> result = new List<UserDTO>();

            if (filtro != null)
            {
                if (filtro.UserName != null)
                {
                    users = users.Where(u => u.UserName.ToLower().Contains(filtro.UserName.ToLower())).ToList();
                }
                if (filtro.Email != null)
                {
                    users = users.Where(u => u.Email.ToLower().Contains(filtro.Email.ToLower())).ToList();
                }
                if (filtro.PhoneNumber != null)
                {
                    users = users.Where(u => u.PhoneNumber.Contains(filtro.PhoneNumber)).ToList();
                }
                if (filtro.Nombre != null)
                {
                    users = users.Where(u => u.nombre.ToLower().Contains(filtro.Nombre.ToLower())).ToList();
                }
                if (filtro.Apellidos != null)
                {
                    users = users.Where(u => u.apellidos.ToLower().Contains(filtro.Apellidos.ToLower())).ToList();
                }
                if (filtro.Nif != null)
                {
                    users = users.Where(u => u.nif.ToLower().Contains(filtro.Nif.ToLower())).ToList();
                }
                if (filtro.FechaNacimiento != null)
                {
                    users = users.Where(u => u.fecha_nacimiento.Contains(filtro.FechaNacimiento)).ToList();
                }
                if (filtro.rol != null)
                {
                    users = users.Where(u => _userManager.GetRolesAsync(u).Result.Contains(filtro.rol)).ToList();
                }
                if (filtro.PharmacyId != 0)
                {
                    users = users.Where(u => u.Pharmacy.Id == filtro.PharmacyId).ToList();
                }

            }   
            // Paginación
            int totalItems = users.Count;
            var paginatedUsers = users
                .Skip((filtro.PageNumber - 1) * filtro.PageSize)
                .Take(filtro.PageSize)
                .ToList();

            foreach (var item in paginatedUsers)
            {
                var user = new UserDTO();
                user.UserName = item.UserName;
                user.Email = item.Email;
                user.Nif = item.nif;
                user.Nombre = item.nombre;
                user.Apellidos = item.apellidos;
                user.PhoneNumber = item.PhoneNumber;
                user.FechaNacimiento = item.fecha_nacimiento;
                user.rol = _userManager.GetRolesAsync(item).Result.FirstOrDefault();
                user.PharmacyId = item.Pharmacy.Id;
                user.Points = item.Points;
                result.Add(user);
            }

            var response = new
            {
                TotalItems = totalItems,
                PageNumber = filtro.PageNumber,
                PageSize = filtro.PageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)filtro.PageSize),
                Items = result
            };

            return Ok(response);
        }

        // GET: api/Usuarios/string
        [HttpGet("Usuario")]
        [Authorize]
        public async Task<ActionResult<UserDTO>> GetUsuario(string id)
        {
            var usuario = await _context.Users.FindAsync(id);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var user = new UserDTO();
            user.UserName = usuario.UserName;
            user.Email = usuario.Email;
            user.Nif = usuario.nif;
            user.Nombre = usuario.nombre;
            user.Apellidos = usuario.apellidos;
            user.PhoneNumber = usuario.PhoneNumber;
            user.FechaNacimiento = usuario.fecha_nacimiento;
            user.rol = _userManager.GetRolesAsync(usuario).Result.FirstOrDefault();

            return Ok(user);
        }

        // PUT: api/Usuarios/5
        [HttpPut("ModificarUsuario")]
        [Authorize]
        public async Task<IActionResult> PutUsuario(string userId, [FromBody] UserDTO dto)
        {
            if (userId == "")
            {
                return BadRequest(new { message = "Debes seleccionar el usuario a modificar" });
            }

            var validation = ValidationsUser(dto);
            if (validation != "ok")
            {
                return BadRequest(new { message = validation });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
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
                
                return NotFound(new { message = "Error al actualizar el usuario" });
                
            }

            return Ok(new { message = "Usuario actualizado correctamente" });
        }

        // DELETE: api/Usuarios/5
        [HttpDelete("EliminarUsuario")]
        [Authorize]
        public async Task<IActionResult> DeleteUsuario(string id)
        {
            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            _context.Users.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado correctamente" });
        }

        [HttpPost("CambiarRol")]
        [Authorize]
        public async Task<IActionResult> CambiarRol(string id, string rol)
        {
            var usuario = await _context.Users.FindAsync(id);
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var roles = await _userManager.GetRolesAsync(usuario);
            await _userManager.RemoveFromRolesAsync(usuario, roles);
            await _userManager.AddToRoleAsync(usuario, rol);

            return Ok(new { message = "Rol del usuario modificado correctamente" });
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
                expires: DateTime.Now.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ValidationsUserRegister(RegisterDTO dto)
        {
            if (dto == null)
            {
                return "No puede enviar el formulario vacio";
            }

            string patternEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            Match matchEmail = Regex.Match(dto.Email, patternEmail);
            if (!matchEmail.Success)
            {
                return "Email no valido";
            }

            string patternNif = @"^(\d{8})([A-Za-z])$";
            Match matchNif = Regex.Match(dto.Nif, patternNif);
            if (!matchNif.Success)
            {
                return "Nif no valido";
            }

            string patternPwd = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{8,15}$";
            Match matchPwd = Regex.Match(dto.Password, patternPwd);
            if (!matchPwd.Success)
            {
                return "Contraseña no valida. (Debe tener 8 caracteres con al menos 1 digito, 1 letra minuscula, 1 letra mayuscula)";
            }

            string patternPhone = @"^(\+34|0034|34)?[6|7|9][0-9]{8}$";
            Match matchPhone = Regex.Match(dto.PhoneNumber, patternPhone);
            if (!matchPhone.Success)
            {
                return "Telefono no valido";
            }

            return "ok";
        }
        private string ValidationsUser(UserDTO dto)
        {
            if (dto == null)
            {
                return "No puede enviar el formulario vacio";
            }

            if (dto.rol != "Admin" && dto.rol != "Farma")
            {
                return "El rol debe ser 'Admin' o 'Farma'.";
            }

            string patternEmail = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$";
            Match matchEmail = Regex.Match(dto.Email, patternEmail);
            if (!matchEmail.Success)
            {
                return "Email no valido";
            }

            string patternNif = @"^(\d{8})([A-Za-z])$";
            Match matchNif = Regex.Match(dto.Nif, patternNif);
            if (!matchNif.Success)
            {
                return "Nif no valido";
            }

            string patternPhone = @"^(\+34|0034|34)?[6|7|9][0-9]{8}$";
            Match matchPhone = Regex.Match(dto.PhoneNumber, patternPhone);
            if (!matchPhone.Success)
            {
                return "Telefono no valido";
            }

            if (dto.PharmacyId == 0)
            {
                return "Debes seleccinar una farmacia valida";
            }

            return "ok";
        }

        private async Task<AuthResult> GenerateJwtAndRefreshToken(IdentityUser user)
        {
            var jwtToken = GenerateJwtToken(user);  // El método que ya tienes para generar el JWT token

            // Generar Refresh Token
            var refreshToken = GenerateRefreshToken();

            // Guardar el Refresh Token en la base de datos o un almacén seguro
            await SaveRefreshToken(user, refreshToken);

            return new AuthResult
            {
                Token = await jwtToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24) // Caducidad corta para el JWT Token
            };
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task SaveRefreshToken(IdentityUser user, string refreshToken)
        {
            var token = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7)  // Caducidad del Refresh Token
            };

            // Guardar en base de datos
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var userId = GetUserIdFromExpiredToken(tokenRequest.Token);
            if (userId == null)
            {
                return BadRequest(new { message = "Token inválido" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await ValidateRefreshToken(user, tokenRequest.RefreshToken))
            {
                return BadRequest(new { message = "Refresh Token inválido o ha expirado" });
            }

            var result = await GenerateJwtAndRefreshToken(user);  // Genera nuevos tokens
            return Ok(result);
        }

        private string GetUserIdFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = false // Deshabilitamos la validación de la caducidad
                }, out SecurityToken securityToken);

                var jwtToken = securityToken as JwtSecurityToken;
                if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Token inválido");
                }

                return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
            catch
            {
                return null;  // Token inválido
            }
        }

        private async Task<bool> ValidateRefreshToken(IdentityUser user, string refreshToken)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == refreshToken && rt.UserId == user.Id);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return false;  // Refresh Token inválido o ha expirado
            }

            return true;
        }

    }


    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RemembeMe { get; set; }
    }

    public class TokenResponse
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
    }

    public class AuthResult
    {
        public string Token { get; set; }            // JWT Token
        public string RefreshToken { get; set; }     // Refresh Token
        public DateTime ExpiresAt { get; set; }      // Fecha de expiración del JWT
    }

    public class TokenRequest
    {
        public string Token { get; set; }
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
