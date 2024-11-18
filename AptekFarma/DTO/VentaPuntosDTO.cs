namespace AptekFarma.DTO
{
    public class VentaPuntosDTO
    {
        public string UserID { get; set; }
        public int ProductID { get; set; }
        public decimal PuntosTotales { get; set; }
        public DateTime FechaCompra { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
