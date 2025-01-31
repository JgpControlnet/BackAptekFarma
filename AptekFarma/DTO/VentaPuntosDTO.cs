using AptekFarma.Models;

namespace AptekFarma.DTO
{
    public class VentaPuntosDTO
    {
        public int? Id { get; set; }
        public string UserID { get; set; }
        public int ProductID { get; set; }
        public decimal PuntosTotales { get; set; }
        public DateTime FechaCompra { get; set; }
        public int? Cantidad { get; set; }
        public ProductoVentaDTO? Product { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
