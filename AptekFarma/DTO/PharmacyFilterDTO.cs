namespace AptekFarma.DTO
{
    public class PharmacyFilterDTO
    {
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
