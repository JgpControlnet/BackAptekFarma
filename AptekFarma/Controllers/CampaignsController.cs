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
    public class CampaignsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Roles> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public CampaignsController(
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

        [HttpGet("GetAllCampaigns")]
        public async Task<IActionResult> GetCampaigns()
        {
            var campaigns = await _context.Campaigns.ToListAsync();
            return Ok(campaigns);
        }

        [HttpGet("GetCampaignById")]
        public async Task<IActionResult> GetCampaignById(int id)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id);

            if (campaign == null)
            {
                return NotFound("No se ha encontrado campaña");
            }

            return Ok(campaign);
        }

        [HttpPost("AddCampaign")]
        public async Task<IActionResult> AddCampaign(CampaignDTO campaign)
        {
            var newCampaign = new Campaign
            {
                Nombre = campaign.Nombre,
                Descripcion = campaign.Descripcion,
                FechaCaducidad = campaign.FechaCaducidad
            };
            //await _context.Campaigns.AddAsync(newCampaign);
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpPut("UpdateCampaign")]
        public async Task<IActionResult> UpdateCampaign(int CampaignId, [FromBody] CampaignDTO campaign)
        {
            var campaignToUpdate = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == CampaignId);

            if (campaignToUpdate == null)
            {
                return NotFound("No se ha encontrado campaña");
            }

            campaignToUpdate.Nombre = campaign.Nombre;
            campaignToUpdate.Descripcion = campaign.Descripcion;
            campaignToUpdate.FechaCaducidad = campaign.FechaCaducidad;

            _context.Campaigns.Update(campaignToUpdate);
            await _context.SaveChangesAsync();
            return Ok(campaign);
        }

        [HttpDelete("DeleteCampaign")]
        public async Task<IActionResult> DeleteCampaign(int id)
        {
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == id);

            if (campaign == null)
            {
                return NotFound("No se ha encontrado campaña");
            }

            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
            return Ok("Campaña borrada correctamente");
        }
    }
}
