using AptekFarma.Models;

namespace AptekFarma.DTO
{
    public class CrearCampannaDTO
    {
        public string nombre { get; set; }
        public string? titulo { get; set; }
        public string? descripcion { get; set; }
        public string? importante { get; set; }
        public string? imagen { get; set; }
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public DateTime fechaValido { get; set; }
        public IFormFile? pdf { get; set; }
        public string? video { get; set; }
        

    }
}
