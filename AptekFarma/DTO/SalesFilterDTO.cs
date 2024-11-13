using _AptekFarma.Models;
using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class SalesFilterDTO
    {
        public string CodigoNacional { get; set; } = "";
        public string Referencia { get; set; } = "";
        public int Nventas { get; set; } = 0;
        public int PonderacionPuntos { get; set; } = 0;
        public int CampaignId { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
