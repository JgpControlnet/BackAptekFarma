using _AptekFarma.Models;
using AptekFarma.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace AptekFarma.DTO
{
    public class SalesFormFilterDTO
    {
        public int ProductId { get; set; } = 0;
        public int Cantidad { get; set; } = 0;
        public string SellerId { get; set; } = "";
        public DateTime Fecha { get; set; }
        public int SaleId { get; set; } = 0;
        public bool Validated { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
