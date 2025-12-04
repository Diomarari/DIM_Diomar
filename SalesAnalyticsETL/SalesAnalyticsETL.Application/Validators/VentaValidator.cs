using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.Validators
{
    public class VentaValidator
    {
        public static bool IsValid(DTOs.VentaDTO venta, out List<string> errors)
        {
            errors = new List<string>();

            if (string.IsNullOrWhiteSpace(venta.OrdenID))
                errors.Add("OrdenID es requerido");

            if (string.IsNullOrWhiteSpace(venta.ClienteNombre))
                errors.Add("ClienteNombre es requerido");

            if (string.IsNullOrWhiteSpace(venta.ProductoNombre))
                errors.Add("ProductoNombre es requerido");

            if (venta.Cantidad <= 0)
                errors.Add("Cantidad debe ser mayor a 0");

            if (venta.Precio <= 0)
                errors.Add("Precio debe ser mayor a 0");

            if (venta.FechaVenta > DateTime.Now.AddDays(1)) 
                errors.Add($"FechaVenta no puede ser futura: {venta.FechaVenta:yyyy-MM-dd}");

            if (venta.FechaVenta < new DateTime(2020, 1, 1))
                errors.Add($"FechaVenta no puede ser anterior a 2020: {venta.FechaVenta:yyyy-MM-dd}");

            
            return errors.Count == 0;
        }

        public static DTOs.VentaDTO Normalize(DTOs.VentaDTO venta)
        {
            venta.OrdenID = venta.OrdenID?.Trim() ?? string.Empty;
            venta.ClienteNombre = venta.ClienteNombre?.Trim().ToUpper() ?? "DESCONOCIDO";
            venta.ClienteApellido = venta.ClienteApellido?.Trim().ToUpper() ?? string.Empty;
            venta.ClienteEmail = venta.ClienteEmail?.Trim().ToLower() ?? string.Empty;
            venta.ProductoNombre = venta.ProductoNombre?.Trim() ?? "DESCONOCIDO";
            venta.Categoria = venta.Categoria?.Trim() ?? "SIN CATEGORIA";
            venta.Estado = venta.Estado?.Trim().ToUpper() ?? "COMPLETADO";

            if (venta.Cantidad <= 0)
                venta.Cantidad = 1; 

            if (venta.Precio <= 0)
                venta.Precio = 0.01m; 

            return venta;
        }
    }
}
