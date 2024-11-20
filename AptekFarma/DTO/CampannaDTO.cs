using AptekFarma.Models;

namespace AptekFarma.DTO
{
    public class CampannaDTO
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public EstadoCampanna? estadoCampanna { get; set; }

    }
}
