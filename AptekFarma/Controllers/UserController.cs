using AptekFarma.Models;
using AptekFarma.DTO;
using AptekFarma.Context;

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


namespace AptekFarma.Controllers
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

            return BadRequest(new { success = false, error = "La solicitud no fue exitosa." });
        }

        [HttpGet("GetAllUsuarios")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsuarios([FromQuery] UserFilterDTO filter)
        {
            var usersQuery = _context.Users
                .Include(u => u.Pharmacy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter.UserName))
            {
                usersQuery = usersQuery.Where(u => u.UserName.Contains(filter.UserName));
            }

            if (!string.IsNullOrEmpty(filter.Email))
            {
                usersQuery = usersQuery.Where(u => u.Email.Contains(filter.Email));
            }

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
            {
                usersQuery = usersQuery.Where(u => u.PhoneNumber.Contains(filter.PhoneNumber));
            }

            if (!string.IsNullOrEmpty(filter.Nombre))
            {
                usersQuery = usersQuery.Where(u => u.nombre.Contains(filter.Nombre));
            }

            if (!string.IsNullOrEmpty(filter.Apellidos))
            {
                usersQuery = usersQuery.Where(u => u.apellidos.Contains(filter.Apellidos));
            }

            if (!string.IsNullOrEmpty(filter.Nif))
            {
                usersQuery = usersQuery.Where(u => u.nif.Contains(filter.Nif));
            }

            if (!string.IsNullOrEmpty(filter.FechaNacimiento))
            {
                usersQuery = usersQuery.Where(u => u.fecha_nacimiento == filter.FechaNacimiento);
            }

            if (!string.IsNullOrEmpty(filter.rol))
            {
                var usersList = await usersQuery.ToListAsync();
                usersList = usersList.Where(u => _userManager.GetRolesAsync(u).Result.Contains(filter.rol)).ToList();
                return Ok(usersList.Select(u => MapToDTO(u)));
            }

            if (filter.PharmacyId != 0)
            {
                usersQuery = usersQuery.Where(u => u.Pharmacy != null && u.Pharmacy.Id == filter.PharmacyId);
            }

            var usersListFiltered = await usersQuery.ToListAsync();
            var usersDTO = usersListFiltered.Select(u => MapToDTO(u)).ToList();

            return Ok(usersDTO);
        }

        [HttpGet("GetAllFarmaceuticos")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetAllFarmaceuticos()
        {
            var users = _context.Users
                .Include(u => u.Pharmacy) 
                .AsQueryable();

            users = users.Where(u => u.PharmacyID != null);

            var usersList = await users.ToListAsync();
            var usersDTO = usersList.Select(u => MapToDTO(u)).ToList();
            return Ok(usersDTO);
        }


        private UserDTO MapToDTO(User user)
        {
            return new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Nombre = user.nombre,
                Apellidos = user.apellidos,
                Nif = user.nif,
                FechaNacimiento = user.fecha_nacimiento,
                rol = _userManager.GetRolesAsync(user).Result.FirstOrDefault(), 
                PharmacyId = user.Pharmacy != null ? user.Pharmacy.Id : null,
                Points = user.Points,
                Pharmacy = user.Pharmacy != null ? new PharmacyDTO
                {
                    id = user.Pharmacy.Id,
                    Nombre = user.Pharmacy.Nombre,
                    Direccion = user.Pharmacy.Direccion,
                    CP = user.Pharmacy.CP,
                    Localidad = user.Pharmacy.Localidad,
                    Provincia = user.Pharmacy.Provincia
                } : null
                
            };
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

            var user = _context.Users.FirstOrDefault(u => u.UserName == dto.UserName);
            if (user != null)
            {
                return "El nombre de usuario ya existe";
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
