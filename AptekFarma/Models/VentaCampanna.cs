using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("venta_campanna")]
    public class VentaCampanna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ProductoCampannaID { get; set; }
        [ForeignKey("ProductoCampannaID")]
        public ProductoCampanna ProductoCampanna { get; set; }
        public int Cantidad { get; set; }
        public double TotalPuntos { get; set; }

        public int FormularioID { get; set; }
        [ForeignKey("FormularioID")]
        public FormularioVentaCampanna FormularioVenta { get; set; }
    }
}
