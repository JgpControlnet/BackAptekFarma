using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AptekFarma.Models
{
    [Table("producto_campanna")]
    public class ProductoCampanna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; }
        public int CampaignId { get; set; }
        [ForeignKey("CampaignId")]
        public Campanna Campanna { get; set; }
        public double Puntos { get; set; }
        public int UnidadesMaximas { get; set; }
        public string Laboratorio { get; set; }
    }
}
