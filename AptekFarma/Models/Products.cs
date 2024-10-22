using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.Models
{
    public class Products
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string CodigoNacional { get; set; }
        public string Nombre { get; set; }
        public string Imagen { get; set; }
        public double Precio { get; set; }
    }
}
