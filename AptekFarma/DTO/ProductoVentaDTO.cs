namespace _AptekFarma.DTO
{
    public class ProductoVentaDTO
    {
        public int Id { get; set; }
        public string nombre { get; set; }
        public int codProducto { get; set; }
        public string? imagen { get; set; }
        public decimal puntosNecesarios { get; set; }
        public int cantidadMax { get; set; }
        public string laboratorio { get; set; }
    }
}
