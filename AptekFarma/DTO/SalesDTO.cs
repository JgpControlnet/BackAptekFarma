using AptekFarma.Models;
using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class SalesDTO
    {
        public string Referencia { get; set; } = "";
        public int Nventas { get; set; } = 0;
        public decimal PonderacionPuntos { get; set; } = 0;
        public int CampaignId { get; set; } = 0;
    }
}
