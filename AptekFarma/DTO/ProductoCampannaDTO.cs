using AptekFarma.Models;

namespace AptekFarma.DTO
{
    public class ProductoCampannaDTO
    {
        public int id { get; set; }
        public string nombre { get; set; }
        //public string? imagen { get; set; }
        public int codigo { get; set; }
        public int campannaId { get; set; }
        public Campanna? campanna { get; set; }
        public double puntos { get; set; }
        public int unidadesMaximas { get; set; }
        public string? laboratorio { get; set; }
    }
}
