﻿namespace _AptekFarma.DTO
{
    public class ProductFilterDTO
    {
        public string? Nombre { get; set; }
        public int PuntosNeceseraios { get; set; } = 0;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
