using SalesAnalyticsETL.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class MockApiClient
    {
        public static List<VentaDTO> GetMockVentas()
        {
            var random = new Random(42);
            var ventas = new List<VentaDTO>();
            var estados = new[] { "COMPLETADO", "PROCESANDO", "PENDIENTE" };

            for (int i = 1; i <= 50; i++)
            {
                ventas.Add(new VentaDTO
                {
                    OrdenID = $"API-{1000 + i}",
                    ClienteNombre = $"Cliente{i}",
                    ClienteApellido = $"Apellido{i}",
                    ClienteEmail = $"cliente{i}@api.com",
                    ProductoNombre = $"Producto API {i % 10 + 1}",
                    Categoria = i % 2 == 0 ? "Electrónica" : "Hogar",
                    Cantidad = random.Next(1, 10),
                    Precio = random.Next(100, 5000),
                    FechaVenta = DateTime.Now.AddDays(-random.Next(1, 180)),
                    Estado = estados[random.Next(estados.Length)]
                });
            }

            return ventas;
        }
    }
}
