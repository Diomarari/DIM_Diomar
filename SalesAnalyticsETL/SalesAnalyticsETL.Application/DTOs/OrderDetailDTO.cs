using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.DTOs
{
    public class OrderDetailDTO
    {
        public string OrdenID { get; set; } = string.Empty;
        public int ClienteID { get; set; }
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; } = string.Empty;
        public decimal Total => Cantidad * Precio;
    }
}
