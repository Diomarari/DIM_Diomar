using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System.Globalization;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class ProductExtractorFromCsv : IExtractor<ProductoDTO>
    {
        private readonly string _csvPath;
        private readonly ILogger<ProductExtractorFromCsv> _logger;

        public ProductExtractorFromCsv(string csvPath, ILogger<ProductExtractorFromCsv> logger)
        {
            _csvPath = csvPath;
            _logger = logger;
        }

        public string GetSourceName() => $"CSV: {Path.GetFileName(_csvPath)}";

        public async Task<IEnumerable<ProductoDTO>> ExtractAsync()
        {
            try
            {
                _logger.LogInformation("Iniciando extracción de productos desde CSV: {path}", _csvPath);

                if (!File.Exists(_csvPath))
                {
                    _logger.LogWarning("Archivo CSV de productos no encontrado: {path}", _csvPath);
                    return Enumerable.Empty<ProductoDTO>();
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

                var productos = csv.GetRecords<ProductoCsvRecord>()
                    .Select(record => new ProductoDTO
                    {
                        ProductoID = record.ProductID,
                        NombreProducto = record.ProductName ?? "DESCONOCIDO",
                        Categoria = record.Category ?? "Sin Categoría",
                        PrecioBase = record.Price,
                        Stock = record.Stock
                    })
                    .ToList();

                _logger.LogInformation("? Extraídos {count} productos desde CSV", productos.Count);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer productos desde CSV: {path}", _csvPath);
                throw;
            }
        }

        private class ProductoCsvRecord
        {
            public int ProductID { get; set; }
            public string? ProductName { get; set; }
            public string? Category { get; set; }
            public decimal Price { get; set; }
            public int Stock { get; set; }
        }
    }
}
