namespace _AptekFarma.DTO
{
    public class ProductFilterDTO
    {
        public string? nombre { get; set; }
        public int puntosNecesarios { get; set; } = 0;
        public int precio { get; set; } = 0;

        //Se puede utilizar tanto para buscar productos de una campaña en especifico como para buscar productos de todas las campañas
        public int campannaId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
