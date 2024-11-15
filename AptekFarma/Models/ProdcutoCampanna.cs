using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AptekFarma.Models
{
    public class ProdcutoCampanna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public Campanna Campanna { get; set; }
        public int CodProducto { get; set; }
        public decimal Puntos { get; set; }
        public int Cantidad { get; set; }
        public string laboratorio { get; set; }
    }
}
