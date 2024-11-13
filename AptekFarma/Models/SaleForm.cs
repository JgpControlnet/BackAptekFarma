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
        public int ProductID { get; set; }
        [ForeignKey("ProductoID")]
        public Products Product { get; set; }
        public int Cantidad { get; set; }
        public string SellerID { get; set; }
        [ForeignKey("VendedorID")]
        public User Seller { get; set; }
        public DateTime Fecha { get; set; }
        public int SaleID { get; set; }
        [ForeignKey("SaleID")]
        public Sale Sale { get; set; }
        public bool Validated { get; set; }

    }
}
