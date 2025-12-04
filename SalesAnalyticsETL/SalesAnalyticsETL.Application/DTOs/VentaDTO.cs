using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.DTOs
{
    public class VentaDTO
    {
        public string OrdenID { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string ClienteApellido { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public string ProductoNombre { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public DateTime FechaVenta { get; set; }
        public string Estado { get; set; } = string.Empty;

        public decimal Total => Cantidad * Precio;
    }
}
