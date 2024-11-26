using AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("venta_campanna_formulario")]
    public class FormularioVentaCampanna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        public User User { get; set; }
     
        public int CampannaID { get; set; }
        [ForeignKey("CampannaID")]
        public Campanna Campanna { get; set; }

        public double TotalPuntos { get; set; }

        public DateTime FechaCreacion { get; set; }

        public int EstadoFormularioID { get; set; }
        [ForeignKey("EstadoFormularioID")]
        public EstadoFormulario EstadoFormulario { get; set; }
    }
}
