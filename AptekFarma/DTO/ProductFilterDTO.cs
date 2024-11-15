namespace _AptekFarma.DTO
{
    public class ProductFilterDTO
    {
        public string? Nombre { get; set; }
        public int Precio { get; set; } = 0;
        public int CampannaId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
