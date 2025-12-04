using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Application.Validators;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Application.Services
{
    public class ETLOrchestrator
    {
        private readonly IEnumerable<IExtractor<VentaDTO>> _extractors;
        private readonly ITransformer<VentaDTO, FactVentas> _transformer;
        private readonly ILoader<FactVentas> _loader;
        private readonly ILogger<ETLOrchestrator> _logger;

        public ETLOrchestrator(
            IEnumerable<IExtractor<VentaDTO>> extractors,
            ITransformer<VentaDTO, FactVentas> transformer,
            ILoader<FactVentas> loader,
            ILogger<ETLOrchestrator> logger)
        {
            _extractors = extractors;
            _transformer = transformer;
            _loader = loader;
            _logger = logger;
        }

        public async Task<ETLResult> ExecuteAsync()
        {
            var result = new ETLResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("=== Iniciando Proceso ETL ===");

                _logger.LogInformation("Fase 1: EXTRACCIÓN");
                var allData = new List<VentaDTO>();

                foreach (var extractor in _extractors)
                {
                    var extractStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation($"Extrayendo datos desde: {extractor.GetSourceName()}");

                    var data = await extractor.ExtractAsync();
                    allData.AddRange(data);

                    extractStopwatch.Stop();
                    _logger.LogInformation(
                        $"✓ Extraídos {data.Count()} registros desde {extractor.GetSourceName()} " +
                        $"en {extractStopwatch.ElapsedMilliseconds}ms");

                    result.ExtractedRecords += data.Count();
                }

                _logger.LogInformation($"Total registros extraídos: {result.ExtractedRecords}");

                _logger.LogInformation("Fase 2: TRANSFORMACIÓN Y VALIDACIÓN");
                var transformStopwatch = Stopwatch.StartNew();

                var validData = new List<VentaDTO>();
                foreach (var venta in allData)
                {
                    var normalized = VentaValidator.Normalize(venta);

                    if (VentaValidator.IsValid(normalized, out var errors))
                    {
                        validData.Add(normalized);
                    }
                    else
                    {
                        result.InvalidRecords++;
                        _logger.LogWarning($"Registro inválido (Orden: {venta.OrdenID}): {string.Join(", ", errors)}");
                    }
                }

                var uniqueData = validData
                    .GroupBy(v => v.OrdenID)
                    .Select(g => g.First())
                    .ToList();

                result.DuplicateRecords = validData.Count - uniqueData.Count;

                if (result.DuplicateRecords > 0)
                {
                    _logger.LogWarning($"Se eliminaron {result.DuplicateRecords} registros duplicados");
                }

                var transformedData = await _transformer.TransformAsync(uniqueData);
                result.TransformedRecords = transformedData.Count();

                transformStopwatch.Stop();
                _logger.LogInformation(
                    $"✓ {result.TransformedRecords} registros transformados y validados " +
                    $"en {transformStopwatch.ElapsedMilliseconds}ms");

                _logger.LogInformation("Fase 3: CARGA AL DATA WAREHOUSE");
                var loadStopwatch = Stopwatch.StartNew();

                result.LoadedRecords = await _loader.LoadAsync(transformedData);

                loadStopwatch.Stop();
                _logger.LogInformation(
                    $"✓ {result.LoadedRecords} registros cargados al Data Warehouse " +
                    $"en {loadStopwatch.ElapsedMilliseconds}ms");

                var verified = await _loader.VerifyLoadAsync();
                if (!verified)
                {
                    _logger.LogWarning("Advertencia: La verificación de carga falló");
                }

                stopwatch.Stop();
                result.TotalTimeMs = stopwatch.ElapsedMilliseconds;
                result.Success = true;

                _logger.LogInformation("=== Proceso ETL Completado Exitosamente ===");
                _logger.LogInformation($"Tiempo total: {result.TotalTimeMs}ms ({result.TotalTimeMs / 1000.0:F2}s)");
                _logger.LogInformation($"Registros procesados: {result.LoadedRecords}/{result.ExtractedRecords}");
                _logger.LogInformation($"Registros inválidos: {result.InvalidRecords}");
                _logger.LogInformation($"Registros duplicados: {result.DuplicateRecords}");
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Error crítico durante el proceso ETL");
            }

            return result;
        }
    }
    }
