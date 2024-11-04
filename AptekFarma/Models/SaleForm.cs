using _AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    public class SaleForm
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("ProductoID")]
        public Products Product { get; set; }
        public int Cantidad { get; set; }
        [ForeignKey("VendedorID")]
        public User Seller { get; set; }
        public DateTime Fecha { get; set; }
        [ForeignKey("CampaignID")]
        public Sale Sale { get; set; }
        public bool Validated { get; set; }

    }
}
