using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;


namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class DatabaseExtractor : IExtractor<VentaDTO>
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseExtractor> _logger;

        public DatabaseExtractor(IConfiguration configuration, ILogger<DatabaseExtractor> logger)
        {
            _connectionString = configuration.GetConnectionString("SourceDatabase")
                ?? throw new ArgumentNullException("SourceDatabase connection string not found");
            _logger = logger;
        }

        public async Task<IEnumerable<VentaDTO>> ExtractAsync()
        {
            var ventas = new List<VentaDTO>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        v.OrdenID,
                        c.Nombre AS ClienteNombre,
                        c.Apellido AS ClienteApellido,
                        c.Email AS ClienteEmail,
                        p.NombreProducto AS ProductoNombre,
                        p.Categoria,
                        v.Cantidad,
                        v.Precio,
                        v.FechaVenta,
                        v.Estado
                    FROM Ventas v
                    INNER JOIN Clientes c ON v.ClienteID = c.ClienteID
                    INNER JOIN Productos p ON v.ProductoID = p.ProductoID
                    WHERE v.FechaVenta >= DATEADD(MONTH, -6, GETDATE())
                    ORDER BY v.FechaVenta DESC";

                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var venta = new VentaDTO
                    {
                        OrdenID = reader.GetString(0),
                        ClienteNombre = reader.GetString(1),
                        ClienteApellido = reader.GetString(2),
                        ClienteEmail = reader.GetString(3),
                        ProductoNombre = reader.GetString(4),
                        Categoria = reader.GetString(5),
                        Cantidad = reader.GetInt32(6),
                        Precio = reader.GetDecimal(7),
                        FechaVenta = reader.GetDateTime(8),
                        Estado = reader.GetString(9)
                    };

                    ventas.Add(venta);
                }

                _logger.LogInformation($"Database: {ventas.Count} registros extraídos exitosamente");
                return ventas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer datos de la base de datos");
                return ventas;
            }
        }

        public string GetSourceName() => "SQL Server Database (Source)";
    }
}
