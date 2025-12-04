using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class DataWarehouseLoader : ILoader<FactVentas>
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DataWarehouseLoader> _logger;

        public DataWarehouseLoader(DataWarehouseContext context, ILogger<DataWarehouseLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> LoadAsync(IEnumerable<FactVentas> data)
        {
            var loadedCount = 0;

            try
            {
                _logger.LogInformation("Iniciando carga optimizada de tabla de hechos FactVentas...");

                var ventasList = data.ToList();

                var ordenesExistentes = await _context.FactVentas
                    .AsNoTracking()
                    .Select(v => v.OrdenID)
                    .ToHashSetAsync();

                _logger.LogInformation("Órdenes existentes en BD: {count}", ordenesExistentes.Count);

                var clienteIdsValidos = await _context.DimClientes
                    .AsNoTracking()
                    .Select(c => c.ClienteID)
                    .ToHashSetAsync();

                var productoIdsValidos = await _context.DimProductos
                    .AsNoTracking()
                    .Select(p => p.ProductoID)
                    .ToHashSetAsync();

                var tiempoIdsValidos = await _context.DimTiempos
                    .AsNoTracking()
                    .Select(t => t.TiempoID)
                    .ToHashSetAsync();

                var estadoIdsValidos = await _context.DimEstados
                    .AsNoTracking()
                    .Select(e => e.EstadoID)
                    .ToHashSetAsync();

                _logger.LogInformation("IDs válidos cargados: Clientes={0}, Productos={1}, Tiempos={2}, Estados={3}",
                    clienteIdsValidos.Count, productoIdsValidos.Count, tiempoIdsValidos.Count, estadoIdsValidos.Count);

                var ventasNuevas = new List<FactVentas>();
                var ventasDuplicadas = 0;
                var ventasClienteInvalido = 0;
                var ventasProductoInvalido = 0;
                var ventasTiempoInvalido = 0;
                var ventasEstadoInvalido = 0;

                foreach (var venta in ventasList)
                {
                    if (ordenesExistentes.Contains(venta.OrdenID))
                    {
                        ventasDuplicadas++;
                        if (ventasDuplicadas <= 5)
                        {
                            _logger.LogDebug($"Venta duplicada (ya existe): {venta.OrdenID}");
                        }
                        continue;
                    }

                    if (!clienteIdsValidos.Contains(venta.ClienteID))
                    {
                        ventasClienteInvalido++;
                        if (ventasClienteInvalido <= 3)
                        {
                            _logger.LogWarning($"Venta {venta.OrdenID} tiene ClienteID inválido: {venta.ClienteID}");
                        }
                        continue;
                    }

                    if (!productoIdsValidos.Contains(venta.ProductoID))
                    {
                        ventasProductoInvalido++;
                        if (ventasProductoInvalido <= 3)
                        {
                            _logger.LogWarning($"Venta {venta.OrdenID} tiene ProductoID inválido: {venta.ProductoID}");
                        }
                        continue;
                    }

                    if (!tiempoIdsValidos.Contains(venta.TiempoID))
                    {
                        ventasTiempoInvalido++;
                        if (ventasTiempoInvalido <= 3)
                        {
                            _logger.LogWarning($"Venta {venta.OrdenID} tiene TiempoID inválido: {venta.TiempoID}");
                        }
                        continue;
                    }

                    if (!estadoIdsValidos.Contains(venta.EstadoID))
                    {
                        ventasEstadoInvalido++;
                        if (ventasEstadoInvalido <= 3)
                        {
                            _logger.LogWarning($"Venta {venta.OrdenID} tiene EstadoID inválido: {venta.EstadoID}");
                        }
                        continue;
                    }

                    ventasNuevas.Add(venta);
                    loadedCount++;
                }

                _logger.LogInformation("=== Resumen de Filtrado ===");
                _logger.LogInformation($"  Total recibido: {ventasList.Count}");
                _logger.LogInformation($"  Duplicadas (ya existen): {ventasDuplicadas}");
                _logger.LogInformation($"  ClienteID inválido: {ventasClienteInvalido}");
                _logger.LogInformation($"  ProductoID inválido: {ventasProductoInvalido}");
                _logger.LogInformation($"  TiempoID inválido: {ventasTiempoInvalido}");
                _logger.LogInformation($"  EstadoID inválido: {ventasEstadoInvalido}");
                _logger.LogInformation($"  NUEVAS a insertar: {ventasNuevas.Count}");

                if (ventasNuevas.Any())
                {
                    const int BATCH_SIZE = 1000;
                    var totalBatches = (int)Math.Ceiling(ventasNuevas.Count / (double)BATCH_SIZE);
                    
                    _logger.LogInformation($"Cargando {ventasNuevas.Count} ventas en {totalBatches} lotes de {BATCH_SIZE}...");

                    for (int i = 0; i < ventasNuevas.Count; i += BATCH_SIZE)
                    {
                        var batch = ventasNuevas.Skip(i).Take(BATCH_SIZE).ToList();
                        var batchNumber = (i / BATCH_SIZE) + 1;

                        await _context.FactVentas.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        
                        _context.ChangeTracker.Clear(); 

                        _logger.LogInformation($"  Lote {batchNumber}/{totalBatches} completado ({batch.Count} registros)");
                    }
                }

                _logger.LogInformation($"✅ Se cargaron {loadedCount} nuevos registros a FactVentas");

                return loadedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos a la tabla de hechos FactVentas");
                throw;
            }
        }

        public async Task<bool> VerifyLoadAsync()
        {
            try
            {
                var totalVentas = await _context.FactVentas.CountAsync();
                var totalClientes = await _context.DimClientes.CountAsync();
                var totalProductos = await _context.DimProductos.CountAsync();
                var totalTiempos = await _context.DimTiempos.CountAsync();
                var totalEstados = await _context.DimEstados.CountAsync();

                _logger.LogInformation("=== Verificación de Carga del DataWarehouse ===");
                _logger.LogInformation("=== DIMENSIONES ===");
                _logger.LogInformation($"Total Clientes: {totalClientes}");
                _logger.LogInformation($"Total Productos: {totalProductos}");
                _logger.LogInformation($"Total Tiempos: {totalTiempos}");
                _logger.LogInformation($"Total Estados: {totalEstados}");
                _logger.LogInformation("=== HECHOS ===");
                _logger.LogInformation($"Total Ventas: {totalVentas}");

                var ventasConClientesInvalidos = await _context.FactVentas
                    .Where(v => !_context.DimClientes.Any(c => c.ClienteID == v.ClienteID))
                    .CountAsync();

                var ventasConProductosInvalidos = await _context.FactVentas
                    .Where(v => !_context.DimProductos.Any(p => p.ProductoID == v.ProductoID))
                    .CountAsync();

                if (ventasConClientesInvalidos > 0 || ventasConProductosInvalidos > 0)
                {
                    _logger.LogWarning($"ADVERTENCIA: Encontradas {ventasConClientesInvalidos} ventas con clientes inválidos");
                    _logger.LogWarning($"ADVERTENCIA: Encontradas {ventasConProductosInvalidos} ventas con productos inválidos");
                }

                var ventasPorEstado = await _context.FactVentas
                    .Include(v => v.Estado)
                    .GroupBy(v => v.Estado.NombreEstado)
                    .Select(g => new { Estado = g.Key, Total = g.Sum(v => v.TotalVenta), Cantidad = g.Count() })
                    .ToListAsync();

                _logger.LogInformation("=== Ventas por Estado ===");
                foreach (var item in ventasPorEstado)
                {
                    _logger.LogInformation($"  {item.Estado}: {item.Cantidad} ventas, Total: ${item.Total:N2}");
                }

                var ventasPorMes = await _context.FactVentas
                    .Include(v => v.Tiempo)
                    .GroupBy(v => new { v.Tiempo.Anio, v.Tiempo.Mes, v.Tiempo.NombreMes })
                    .Select(g => new 
                    { 
                        Anio = g.Key.Anio, 
                        Mes = g.Key.Mes,
                        NombreMes = g.Key.NombreMes,
                        Total = g.Sum(v => v.TotalVenta), 
                        Cantidad = g.Count() 
                    })
                    .OrderBy(x => x.Anio)
                    .ThenBy(x => x.Mes)
                    .ToListAsync();

                _logger.LogInformation("=== Ventas por Mes ===");
                foreach (var item in ventasPorMes)
                {
                    _logger.LogInformation($"  {item.NombreMes} {item.Anio}: {item.Cantidad} ventas, Total: ${item.Total:N2}");
                }

                return ventasConClientesInvalidos == 0 && ventasConProductosInvalidos == 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la carga");
                return false;
            }
        }
    }
}
