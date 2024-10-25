using _AptekFarma.Models;
using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class SalesFilterDTO
    {
        public int ProductoId { get; set; } = 0;
        public int Cantidad { get; set; } = 0;
        public int CampaignId { get; set; } = 0;
        public string VendedorId { get; set; } = "";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
