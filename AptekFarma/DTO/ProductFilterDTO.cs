﻿namespace _AptekFarma.DTO
{
    public class ProductFilterDTO
    {
        public string? Nombre { get; set; }
        public int PuntosNeceseraios { get; set; } = 0;
        public int Precio { get; set; } = 0;

        //Se puede utilizar tanto para buscar productos de una campaña en especifico como para buscar productos de todas las campañas
        public int CampannaId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool Todas { get; set; }
    }
}
