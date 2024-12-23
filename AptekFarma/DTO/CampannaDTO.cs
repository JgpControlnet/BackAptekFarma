using AptekFarma.Models;

namespace AptekFarma.DTO
{
    public class CampannaDTO
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public string? importante { get; set; }
        public string? imagen { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public DateTime fechaValido { get; set; }
        public EstadoCampanna? estadoCampanna { get; set; }
        public int? informesConfirmados { get; set; }
        public int? informesPendientes { get; set; }
        public double? puntosObtenidos { get; set; }
        public string? PDF { get; set; }
        public string? Video { get; set; }

    }
}
