using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class CsvExtractor : IExtractor<VentaDTO>
    {
        private readonly string _csvFilePath;
        private readonly ILogger<CsvExtractor> _logger;

        public CsvExtractor(string csvFilePath, ILogger<CsvExtractor> logger)
        {
            _csvFilePath = csvFilePath;
            _logger = logger;
        }

        public async Task<IEnumerable<VentaDTO>> ExtractAsync()
        {
            try
            {
                var ventas = new List<VentaDTO>();

                if (!File.Exists(_csvFilePath))
                {
                    _logger.LogWarning($"Archivo CSV no encontrado: {_csvFilePath}");
                    return ventas;
                }

                using var reader = new StreamReader(_csvFilePath);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    MissingFieldFound = null,
                    BadDataFound = null
                };

                using var csv = new CsvReader(reader, config);

                await Task.Run(() =>
                {
                    csv.Read();
                    csv.ReadHeader();

                    while (csv.Read())
                    {
                        try
                        {
                            var venta = new VentaDTO
                            {
                                OrdenID = csv.GetField<string>("OrdenID") ?? string.Empty,
                                ClienteNombre = csv.GetField<string>("ClienteNombre") ?? string.Empty,
                                ClienteApellido = csv.GetField<string>("ClienteApellido") ?? string.Empty,
                                ClienteEmail = csv.GetField<string>("ClienteEmail") ?? string.Empty,
                                ProductoNombre = csv.GetField<string>("ProductoNombre") ?? string.Empty,
                                Categoria = csv.GetField<string>("Categoria") ?? string.Empty,
                                Cantidad = csv.GetField<int>("Cantidad"),
                                Precio = csv.GetField<decimal>("Precio"),
                                FechaVenta = csv.GetField<DateTime>("FechaVenta"),
                                Estado = csv.GetField<string>("Estado") ?? "COMPLETADO"
                            };

                            ventas.Add(venta);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Error al leer fila del CSV: {ex.Message}");
                        }
                    }
                });

                _logger.LogInformation($"CSV: {ventas.Count} registros extraídos exitosamente");
                return ventas;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al extraer datos del CSV: {_csvFilePath}");
                throw;
            }
        }

        public string GetSourceName() => $"CSV File ({Path.GetFileName(_csvFilePath)})";
    }
}
