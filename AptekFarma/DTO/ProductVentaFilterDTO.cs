namespace AptekFarma.DTO
{
    public class ProductVentaFilterDTO
    {
        public string? nombre { get; set; }
        public int? puntosDesde { get; set; } 
        public int? puntosHasta { get; set; } 
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
