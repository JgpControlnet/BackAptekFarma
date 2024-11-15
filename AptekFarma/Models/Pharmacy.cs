using _AptekFarma.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    [Table("pharmacy")]
    public class Pharmacy
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string? CP { get; set; }
        public string? Imagen { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }


        //public int? LocalidadID { get; set; }
        //[ForeignKey("LocalidadID")]
        //public Localidad Localidad { get; set; }

        //public int? ProvinciaID { get; set; }
        //[ForeignKey("ProvinciaID")]
        //public Provincia Provincia { get; set; }


    }
}
