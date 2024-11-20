using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("producto_venta")]
    public class ProductoVenta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int CodProducto{ get; set; }
        public string? Imagen { get; set; }
        public decimal PuntosNecesarios { get; set; }
        public int CantidadMax { get; set; }
        public string Laboratorio { get; set; }

    }
}
