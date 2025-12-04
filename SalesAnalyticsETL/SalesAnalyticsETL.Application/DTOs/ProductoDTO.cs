using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.DTOs
{
    public class ProductoDTO
    {
        public int ProductoID { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal PrecioBase { get; set; }
        public int Stock { get; set; }
    }
}
