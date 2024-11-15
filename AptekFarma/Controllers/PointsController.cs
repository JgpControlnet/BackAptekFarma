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
    public class PointsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PointsController(
            UserManager<User> userManager,
            RoleManager<Roles> roleManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
        }

        //[HttpPut("BuyProduct")]
        //public async Task<IActionResult> BuyProduct([FromBody] string userId, int productId)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        //    if (user == null)
        //    {
        //        return BadRequest(new { message = "Usuario no encontrado" });
        //    }

        //    var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == productId);
        //    if (product == null)
        //    {
        //        return BadRequest(new { message = "Producto no encontrado" });
        //    }

        //    if (product.PuntosNeceseraios > user.Points)
        //    {
        //        return BadRequest(new { message = "Sin puntos suficientes" });
        //    }

        //    user.Points -= product.PuntosNeceseraios;
        //    var pointsRedeemed = new PointRedeemded();
        //    pointsRedeemed.User = user;
        //    pointsRedeemed.Product = product;
        //    pointsRedeemed.Points = product.PuntosNeceseraios;
        //    pointsRedeemed.Fecha = DateTime.Now;

        //    await _userManager.UpdateAsync(user);
        //    await _context.PointsRedeemded.AddAsync(pointsRedeemed);
        //    await _context.SaveChangesAsync();

        //    return Ok("Product bought successfully");
        //}

        [HttpGet("GetUserPoints")]
        public async Task<IActionResult> GetUserPoints(string userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return BadRequest(new { message = "Usuario no encontrado" });
            }

            return Ok(user.Points);
        }

        //[HttpGet("GetUserPointsHistory")]
        //public async Task<IActionResult> GetUserPointsHistory(string userId)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        //    if (user == null)
        //    {
        //        return BadRequest(new { message = "User not found" });
        //    }

        //    var pointsEarnedHistory = await _context.PointsEarned
        //        .Where(x => x.User.Id == userId)
        //        .Select(x => new
        //        {
        //            x.Id,
        //            x.Points,
        //            x.Fecha,
        //        })
        //        .ToListAsync();

        //    var pointsRedeemedHistory = await _context.PointsRedeemded
        //        .Where(x => x.User.Id == userId)
        //        .Select(x => new
        //        {
        //            x.Id,
        //            x.Product,
        //            x.Points,
        //            x.Fecha,
        //        })
        //        .ToListAsync();

        //    var pointsHistory = new
        //    {
        //        PointsEarnedHistory = pointsEarnedHistory,
        //        PointsRedeemedHistory = pointsRedeemedHistory
        //    };

        //    return Ok(pointsHistory);
        //}
    }
}
