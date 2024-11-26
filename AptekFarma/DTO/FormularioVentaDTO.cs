using AptekFarma.Models;
namespace AptekFarma.DTO
{
    public class FormularioVentaDTO
    {
        public int id { get; set; }
        public string userID { get; set; }
        public UserDTO? user { get; set; }
        public int? estadoFormularioID { get; set; }
        public EstadoFormulario? estadoFormulario { get; set; }
        public int campannaID { get; set; }
        public Campanna? campanna { get; set; }
        public IEnumerable<VentaCampanna?>? ventaCampannas { get; set; }
        public double totalPuntos { get; set; }
        public DateTime? fechaCreacion { get; set; }
        public Pharmacy? farmacia { get; set; }
        internal IEnumerable<Object> Rankings { get; set; }

        //añadir fecha de subida
    }
}
