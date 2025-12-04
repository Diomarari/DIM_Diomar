using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System.Globalization;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class OrderDetailExtractorFromCsv : IExtractor<OrderDetailDTO>
    {
        private readonly string _csvPath;
        private readonly ILogger<OrderDetailExtractorFromCsv> _logger;

        public OrderDetailExtractorFromCsv(string csvPath, ILogger<OrderDetailExtractorFromCsv> logger)
        {
            _csvPath = csvPath;
            _logger = logger;
        }

        public string GetSourceName() => $"CSV: {Path.GetFileName(_csvPath)}";

        public async Task<IEnumerable<OrderDetailDTO>> ExtractAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando extracción de detalles de órdenes desde CSV: {path}", _csvPath);

                if (!File.Exists(_csvPath))
                {
                    _logger.LogWarning("Archivo CSV de order details no encontrado: {path}", _csvPath);
                    return Enumerable.Empty<OrderDetailDTO>();
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null,
                    TrimOptions = TrimOptions.Trim
                };

                using var reader = new StreamReader(_csvPath);
                using var csv = new CsvReader(reader, config);

                var orderDetails = csv.GetRecords<OrderDetailCsvRecord>()
                    .Select(record => new OrderDetailDTO
                    {
                        OrdenID = record.OrderID ?? "UNKNOWN",
                        ClienteID = 0,
                        ProductoID = record.ProductID,
                        Cantidad = record.Quantity,
                        Precio = record.TotalPrice / Math.Max(record.Quantity, 1),
                        FechaVenta = DateTime.Today,
                        Estado = "Pendiente"
                    })
                    .ToList();

                _logger.LogInformation("? Extraídos {count} detalles de órdenes desde CSV", orderDetails.Count);
                _logger.LogWarning("NOTA: CSV de order_details no contiene ClienteID, FechaVenta ni Estado. Se usan valores por defecto.");

                return orderDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer order details desde CSV: {path}", _csvPath);
                throw;
            }
        }

        private class OrderDetailCsvRecord
        {
            public string? OrderID { get; set; }
            public int ProductID { get; set; }
            public int Quantity { get; set; }
            public decimal TotalPrice { get; set; }
        }
    }
}
