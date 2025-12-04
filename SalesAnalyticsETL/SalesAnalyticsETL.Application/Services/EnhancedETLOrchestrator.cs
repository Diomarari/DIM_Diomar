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
    public class EnhancedETLOrchestrator
    {
        private readonly IEnumerable<IExtractor<VentaDTO>> _ventaExtractors;
        private readonly IEnumerable<IExtractor<ClienteDTO>> _clienteExtractors;
        private readonly IEnumerable<IExtractor<ProductoDTO>> _productoExtractors;
        private readonly IEnumerable<IExtractor<OrderDetailDTO>> _orderDetailExtractors;
        private readonly ITransformer<VentaDTO, FactVentas> _transformer;
        private readonly ILoader<FactVentas> _loader;
        private readonly IDimClienteLoader _clienteLoader;
        private readonly IDimProductoLoader _productoLoader;
        private readonly IDimTiempoLoader _tiempoLoader;
        private readonly IDimEstadoLoader _estadoLoader;
        private readonly ILogger<EnhancedETLOrchestrator> _logger;

        public EnhancedETLOrchestrator(
            IEnumerable<IExtractor<VentaDTO>> ventaExtractors,
            IEnumerable<IExtractor<ClienteDTO>> clienteExtractors,
            IEnumerable<IExtractor<ProductoDTO>> productoExtractors,
            IEnumerable<IExtractor<OrderDetailDTO>> orderDetailExtractors,
            ITransformer<VentaDTO, FactVentas> transformer,
            ILoader<FactVentas> loader,
            IDimClienteLoader clienteLoader,
            IDimProductoLoader productoLoader,
            IDimTiempoLoader tiempoLoader,
            IDimEstadoLoader estadoLoader,
            ILogger<EnhancedETLOrchestrator> logger)
        {
            _ventaExtractors = ventaExtractors;
            _clienteExtractors = clienteExtractors;
            _productoExtractors = productoExtractors;
            _orderDetailExtractors = orderDetailExtractors;
            _transformer = transformer;
            _loader = loader;
            _clienteLoader = clienteLoader;
            _productoLoader = productoLoader;
            _tiempoLoader = tiempoLoader;
            _estadoLoader = estadoLoader;
            _logger = logger;
        }

        public async Task<ETLResult> ExecuteAsync()
        {
            var result = new ETLResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("=== Iniciando Proceso ETL Mejorado (Múltiples Fuentes) ===");

                _logger.LogInformation("Fase 1: EXTRACCIÓN DESDE MÚLTIPLES FUENTES");

                var allClientes = new List<ClienteDTO>();
                foreach (var extractor in _clienteExtractors)
                {
                    var extractStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation($"Extrayendo clientes desde: {extractor.GetSourceName()}");
                    var data = await extractor.ExtractAsync();
                    allClientes.AddRange(data);
                    extractStopwatch.Stop();
                    _logger.LogInformation($"✓ Extraídos {data.Count()} clientes en {extractStopwatch.ElapsedMilliseconds}ms");
                }

                _logger.LogInformation($"📊 Total clientes extraídos: {allClientes.Count}");
                _logger.LogInformation($"📧 Clientes con Email: {allClientes.Count(c => !string.IsNullOrWhiteSpace(c.Email))}");
                _logger.LogInformation($"🔢 Clientes con ID > 0: {allClientes.Count(c => c.ClienteID > 0)}");

                var allProductos = new List<ProductoDTO>();
                foreach (var extractor in _productoExtractors)
                {
                    var extractStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation($"Extrayendo productos desde: {extractor.GetSourceName()}");
                    var data = await extractor.ExtractAsync();
                    allProductos.AddRange(data);
                    extractStopwatch.Stop();
                    _logger.LogInformation($"✓ Extraídos {data.Count()} productos en {extractStopwatch.ElapsedMilliseconds}ms");
                }

                _logger.LogInformation($"📊 Total productos extraídos: {allProductos.Count}");
                _logger.LogInformation($"📦 Productos con Nombre: {allProductos.Count(p => !string.IsNullOrWhiteSpace(p.NombreProducto))}");
                _logger.LogInformation($"🔢 Productos con ID > 0: {allProductos.Count(p => p.ProductoID > 0)}");

                var allOrderDetails = new List<OrderDetailDTO>();
                foreach (var extractor in _orderDetailExtractors)
                {
                    var extractStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation($"Extrayendo detalles de órdenes desde: {extractor.GetSourceName()}");
                    var data = await extractor.ExtractAsync();
                    allOrderDetails.AddRange(data);
                    extractStopwatch.Stop();
                    _logger.LogInformation($"✓ Extraídos {data.Count()} detalles de órdenes en {extractStopwatch.ElapsedMilliseconds}ms");
                }

                var allVentas = new List<VentaDTO>();
                foreach (var extractor in _ventaExtractors)
                {
                    var extractStopwatch = Stopwatch.StartNew();
                    _logger.LogInformation($"Extrayendo ventas desde: {extractor.GetSourceName()}");
                    var data = await extractor.ExtractAsync();
                    allVentas.AddRange(data);
                    extractStopwatch.Stop();
                    _logger.LogInformation($"✓ Extraídos {data.Count()} registros de ventas en {extractStopwatch.ElapsedMilliseconds}ms");
                }

                _logger.LogInformation("Fase 2: CARGA DE DIMENSIONES");

                var dimClientesStopwatch = Stopwatch.StartNew();
                var clientesCargados = await _clienteLoader.LoadDimensionAsync(allClientes);
                dimClientesStopwatch.Stop();
                _logger.LogInformation($"✓ Dimensión Clientes cargada: {clientesCargados} nuevos en {dimClientesStopwatch.ElapsedMilliseconds}ms");

                var dimProductosStopwatch = Stopwatch.StartNew();
                var productosCargados = await _productoLoader.LoadDimensionAsync(allProductos);
                dimProductosStopwatch.Stop();
                _logger.LogInformation($"✓ Dimensión Productos cargada: {productosCargados} nuevos en {dimProductosStopwatch.ElapsedMilliseconds}ms");

                var dimTiempoStopwatch = Stopwatch.StartNew();
                var fechasVentas = allVentas.Select(v => v.FechaVenta).Concat(allOrderDetails.Select(o => o.FechaVenta));
                var tiemposCargados = await _tiempoLoader.LoadDimensionForDateRangeAsync(fechasVentas);
                dimTiempoStopwatch.Stop();
                _logger.LogInformation($"✓ Dimensión Tiempo cargada: {tiemposCargados} fechas nuevas en {dimTiempoStopwatch.ElapsedMilliseconds}ms");

                var dimEstadoStopwatch = Stopwatch.StartNew();
                var estadosUnicos = allVentas
                    .Select(v => v.Estado)
                    .Concat(allOrderDetails.Select(o => o.Estado))
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .Distinct()
                    .ToList();

                _logger.LogInformation($"Estados únicos encontrados: {string.Join(", ", estadosUnicos)}");

                var estadosCargados = 0;
                foreach (var nombreEstado in estadosUnicos)
                {
                    try
                    {
                        await _estadoLoader.GetOrCreateEstadoByNameAsync(nombreEstado);
                        estadosCargados++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al cargar estado '{nombreEstado}': {ex.Message}");
                    }
                }
                dimEstadoStopwatch.Stop();
                _logger.LogInformation($"✓ Dimensión Estados cargada: {estadosCargados} estados en {dimEstadoStopwatch.ElapsedMilliseconds}ms");

                _logger.LogInformation("Fase 3: CONSOLIDACIÓN Y ENRIQUECIMIENTO");

                var clientesPorId = allClientes
                    .Where(c => c.ClienteID > 0)
                    .GroupBy(c => c.ClienteID)
                    .ToDictionary(g => g.Key, g => g.First());

                var productosPorId = allProductos
                    .Where(p => p.ProductoID > 0)
                    .GroupBy(p => p.ProductoID)
                    .ToDictionary(g => g.Key, g => g.First());

                _logger.LogInformation($"Diccionarios creados: {clientesPorId.Count} clientes únicos, {productosPorId.Count} productos únicos");

                var ventasEnriquecidas = 0;
                foreach (var detail in allOrderDetails)
                    {
                    try
                    {
                        var clienteInfo = clientesPorId.ContainsKey(detail.ClienteID)
                            ? clientesPorId[detail.ClienteID]
                            : null;

                        var productoInfo = productosPorId.ContainsKey(detail.ProductoID)
                            ? productosPorId[detail.ProductoID]
                            : null;

                        var venta = new VentaDTO
                        {
                            OrdenID = detail.OrdenID,
                            ClienteNombre = clienteInfo?.Nombre ?? "DESCONOCIDO",
                            ClienteApellido = clienteInfo?.Apellido ?? "",
                            ClienteEmail = clienteInfo?.Email ?? "",
                            ProductoNombre = productoInfo?.NombreProducto ?? "DESCONOCIDO",
                            Categoria = productoInfo?.Categoria ?? "",
                            Cantidad = detail.Cantidad,
                            Precio = detail.Precio,
                            FechaVenta = detail.FechaVenta,
                            Estado = detail.Estado
                        };
                        allVentas.Add(venta);
                        ventasEnriquecidas++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error al consolidar orden {detail.OrdenID}: {ex.Message}");
                    }
                }

                _logger.LogInformation($"Ventas enriquecidas desde OrderDetails: {ventasEnriquecidas}");

                result.ExtractedRecords = allVentas.Count;
                _logger.LogInformation($"Total registros consolidados: {result.ExtractedRecords}");
                _logger.LogInformation($"Clientes únicos: {allClientes.Count}");
                _logger.LogInformation($"Productos únicos: {allProductos.Count}");

                _logger.LogInformation("Fase 4: TRANSFORMACIÓN Y VALIDACIÓN");
                var transformStopwatch = Stopwatch.StartNew();

                var validData = new List<VentaDTO>();
                var invalidSamples = new List<string>();
                
                foreach (var venta in allVentas)
                {
                    var normalized = VentaValidator.Normalize(venta);
                    if (VentaValidator.IsValid(normalized, out var errors))
                    {
                        validData.Add(normalized);
                    }
                    else
                    {
                        result.InvalidRecords++;
                        var errorMsg = $"Orden: {venta.OrdenID}, Errores: {string.Join(", ", errors)}";
                        
                        if (result.InvalidRecords <= 10)
                        {
                            _logger.LogWarning($"Registro inválido: {errorMsg}");
                        }
                        else if (result.InvalidRecords == 11)
                        {
                            _logger.LogWarning($"... y {allVentas.Count - validData.Count - 10} registros inválidos más (se omiten logs para no saturar)");
                        }
                    }
                }

                _logger.LogInformation($"Registros válidos después de normalización: {validData.Count}/{allVentas.Count}");

                var ordenesUnicas = validData
                    .GroupBy(v => v.OrdenID)
                    .ToList();

                var uniqueData = ordenesUnicas
                    .Select(g => g.First())
                    .ToList();

                result.DuplicateRecords = validData.Count - uniqueData.Count;
                
                if (result.DuplicateRecords > 0)
                {
                    _logger.LogWarning($"Se eliminaron {result.DuplicateRecords} registros duplicados (misma OrdenID)");
                    
                    var duplicadosEjemplos = ordenesUnicas
                        .Where(g => g.Count() > 1)
                        .Take(5)
                        .Select(g => $"OrdenID: {g.Key} ({g.Count()} veces)")
                        .ToList();
                    
                    if (duplicadosEjemplos.Any())
                    {
                        _logger.LogInformation($"Ejemplos de duplicados: {string.Join(", ", duplicadosEjemplos)}");
                    }
                }

                var transformedData = await _transformer.TransformAsync(uniqueData);
                result.TransformedRecords = transformedData.Count();
                transformStopwatch.Stop();

                _logger.LogInformation($"✓ {result.TransformedRecords} registros transformados y validados en {transformStopwatch.ElapsedMilliseconds}ms");

                _logger.LogInformation("Fase 5: CARGA DE TABLA DE HECHOS (FactVentas)");
                var loadStopwatch = Stopwatch.StartNew();

                result.LoadedRecords = await _loader.LoadAsync(transformedData);
                loadStopwatch.Stop();

                _logger.LogInformation($"✓ {result.LoadedRecords} registros cargados al Data Warehouse en {loadStopwatch.ElapsedMilliseconds}ms");

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
                _logger.LogInformation($"Dimensiones cargadas: Clientes={clientesCargados}, Productos={productosCargados}, Tiempos={tiemposCargados}, Estados={estadosCargados}");
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
