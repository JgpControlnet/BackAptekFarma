using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;
namespace AptekFarma.DTO
{
    public class VentaCampannaDTO
    {
        public int id { get; set; }
        public int? productoCampannaID { get; set; }
        public int? cantidad { get; set; }
        public double? totalPuntos { get; set; }
        //añadir fecha de subida
    }
}
