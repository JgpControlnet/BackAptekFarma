﻿using _AptekFarma.Models;
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
    [Route("/[controller]")]
    [ApiController]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<User> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public SalesController(
            UserManager<User> userManager,
            RoleManager<User> roleManager,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext context,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("GetAllSales")]
        public async Task<IActionResult> GetSales()
        {
            var sales = await _context.Sales.ToListAsync();
            return Ok(sales);
        }

        [HttpGet("GetSaleById")]
        public async Task<IActionResult> GetSaleById(int id)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == id);
            return Ok(sale);
        }

        [HttpPost("CreateSale")]
        public async Task<IActionResult> CreateSale(SalesDTO sale)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == sale.ProductoId);
            var seller = await _context.Users.FirstOrDefaultAsync(x => x.Id == sale.VendedorId);
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (product == null || seller == null || campaign == null)
            {
                return BadRequest("Invalid product, seller or campaign");
            }

            var newSale = new Sales
            {
                Product = product,
                Cantidad = sale.Cantidad,
                Seller = seller,
                Fecha = DateTime.Now,
                Campaign = campaign
            };

            _context.Sales.Add(newSale);
            await _context.SaveChangesAsync();

            return Ok(newSale);
        }

        [HttpPut("UpdateSale")]
        public async Task<IActionResult> UpdateSale(SalesDTO sale)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == sale.ProductoId);
            var seller = await _context.Users.FirstOrDefaultAsync(x => x.Id == sale.VendedorId);
            var campaign = await _context.Campaigns.FirstOrDefaultAsync(x => x.Id == sale.CampaignId);

            if (product == null || seller == null || campaign == null)
            {
                return BadRequest("Invalid product, seller or campaign");
            }

            var saleToUpdate = await _context.Sales.FirstOrDefaultAsync(x => x.Id == sale.Id);

            if (saleToUpdate == null)
            {
                return NotFound();
            }

            saleToUpdate.Product = product;
            saleToUpdate.Cantidad = sale.Cantidad;
            saleToUpdate.Seller = seller;
            saleToUpdate.Campaign = campaign;

            await _context.SaveChangesAsync();

            return Ok(saleToUpdate);
        }

        [HttpDelete("DeleteSale")]
        public async Task<IActionResult> DeleteSale(int id)
        {
            var sale = await _context.Sales.FirstOrDefaultAsync(x => x.Id == id);

            if (sale == null)
            {
                return NotFound();
            }

            _context.Sales.Remove(sale);
            await _context.SaveChangesAsync();

            return Ok(sale);
        }

    }
}
