using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    public class Sale
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CodigoNacional { get; set; }
        public string Referencia { get; set; }
        public int Nventas { get; set; }
        public int PonderacionPuntos { get; set; }
        [ForeignKey("CampaignID")]
        public Campaign Campaign { get; set; }

    }
}
