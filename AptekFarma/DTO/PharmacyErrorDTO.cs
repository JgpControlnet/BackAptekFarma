using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class PharmacyErrorDTO
    {
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string CP { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }
        public int Linea { get; set; }
        public List<string> Errores { get; set; }

    }
}
