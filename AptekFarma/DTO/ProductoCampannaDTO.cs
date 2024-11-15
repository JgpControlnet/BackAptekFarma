using AptekFarma.Models;

namespace _AptekFarma.DTO
{
    public class ProductoCampannaDTO
    {
        public string nombre { get; set; }
        public string? imagen { get; set; }
        public int codigo { get; set; }
        public int campaignId { get; set; }
        public Campanna? campanna { get; set; }
        public double puntos { get; set; }
        public int unidadesMaximas { get; set; }
        public string? laboratorio { get; set; }
    }
}
