using _AptekFarma.Models;
using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class SalesDTO
    {
        public int? Id { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public int CampaignId { get; set; }
        public string VendedorId { get; set; }
    }
}
