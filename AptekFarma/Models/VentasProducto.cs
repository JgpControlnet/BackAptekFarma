using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AptekFarma.Models
{
    public class VentasProducto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int IdProducto { get; set; }
        [ForeignKey("IdProducto")]
        public Products Producto { get; set; }

    }
}
