using AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("ventas_puntos")]
    public class VentaPuntos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
        public int ProductID { get; set; }
        [ForeignKey("ProductID")]
        public ProductoVenta Product { get; set; }
        public int Cantidad { get; set; }
        public decimal PuntosTotales { get; set; }
        public DateTime FechaCompra { get; set; }

    }
}
