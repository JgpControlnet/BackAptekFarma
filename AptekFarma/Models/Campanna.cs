using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("campanna")]
    public class Campanna
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string Importante { get; set; }
        public string? Imagen { get; set; }
        public string Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public DateTime FechaValido { get; set; }

        public int  EstadoCampannaId { get; set; }
        [ForeignKey("EstadoCampannaId")]
        public EstadoCampanna? EstadoCampanna { get; set; }
        public string? PDF { get; set; }
        public string? Video { get; set; }
        public bool Activo { get; set; }

    }
}
