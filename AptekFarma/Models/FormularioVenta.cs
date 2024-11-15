using _AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("formulario_venta")]
    public class FormularioVenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        public int ProductID { get; set; }
        [ForeignKey("ProductID")]
        public ProductVenta Product { get; set; }
        public int Cantidad { get; set; }
    }
}
