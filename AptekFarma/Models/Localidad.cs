using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    public class Localidad
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public int ProvinciaID { get; set; }
        [ForeignKey("ProvinciaID")]
        public Provincia Provincia { get; set; }
    }
}
