using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class VentasTransformer : ITransformer<VentaDTO, FactVentas>
    {
        private readonly DataWarehouseContext _context;
        private readonly IDimClienteLoader _clienteLoader;
        private readonly IDimProductoLoader _productoLoader;
        private readonly IDimTiempoLoader _tiempoLoader;
        private readonly ILogger<VentasTransformer> _logger;

        public VentasTransformer(
            DataWarehouseContext context,
            IDimClienteLoader clienteLoader,
            IDimProductoLoader productoLoader,
            IDimTiempoLoader tiempoLoader,
            ILogger<VentasTransformer> logger)
        {
            _context = context;
            _clienteLoader = clienteLoader;
            _productoLoader = productoLoader;
            _tiempoLoader = tiempoLoader;
            _logger = logger;
        }

        public async Task<IEnumerable<FactVentas>> TransformAsync(IEnumerable<VentaDTO> data)
        {
            var factVentas = new List<FactVentas>();
            var ventas = data.ToList();

            _logger.LogInformation($"Iniciando transformación de {ventas.Count} registros...");

            _logger.LogInformation("Pre-cargando dimensiones en memoria...");

            var clientesDict = await _context.DimClientes
                .AsNoTracking()
                .ToDictionaryAsync(
                    c => c.Email.ToLower(), 
                    c => c.ClienteID
                );

            var productosDict = await _context.DimProductos
                .AsNoTracking()
                .ToDictionaryAsync(
                    p => p.NombreProducto.ToLower(), 
                    p => p.ProductoID
                );

            var tiemposDict = await _context.DimTiempos
                .AsNoTracking()
                .ToDictionaryAsync(
                    t => t.Fecha.Date, 
                    t => t.TiempoID
                );

            var estadosDict = await _context.DimEstados
                .AsNoTracking()
                .ToDictionaryAsync(
                    e => e.NombreEstado.ToUpper(), 
                    e => e.EstadoID
                );

            _logger.LogInformation($"Dimensiones cargadas: Clientes={clientesDict.Count}, Productos={productosDict.Count}, Tiempos={tiemposDict.Count}, Estados={estadosDict.Count}");

            var clienteDesconocidoID = await _clienteLoader.GetOrCreateUnknownClienteAsync();
            var productoDesconocidoID = await _productoLoader.GetOrCreateUnknownProductoAsync();
            var estadoCompletadoID = estadosDict.ContainsKey("COMPLETADO") ? estadosDict["COMPLETADO"] : 3;

            var clientesNoEncontrados = 0;
            var productosNoEncontrados = 0;
            var tiemposNoEncontrados = 0;

            foreach (var venta in ventas)
            {
                try
                {
                    var emailKey = venta.ClienteEmail?.ToLower() ?? "";
                    int clienteID;
                    
                    if (string.IsNullOrWhiteSpace(emailKey) || !clientesDict.TryGetValue(emailKey, out clienteID))
                    {
                        clienteID = clienteDesconocidoID;
                        clientesNoEncontrados++;
                    }

                    var productoKey = venta.ProductoNombre?.ToLower() ?? "";
                    int productoID;
                    
                    if (string.IsNullOrWhiteSpace(productoKey) || !productosDict.TryGetValue(productoKey, out productoID))
                    {
                        productoID = productoDesconocidoID;
                        productosNoEncontrados++;
                    }

                    var fechaKey = venta.FechaVenta.Date;
                    int tiempoID;
                    
                    if (!tiemposDict.TryGetValue(fechaKey, out tiempoID))
                    {
                        tiempoID = await _tiempoLoader.LoadDimensionForDateAsync(venta.FechaVenta);
                        tiemposDict[fechaKey] = tiempoID;
                        tiemposNoEncontrados++;
                    }

                    var estadoKey = venta.Estado?.ToUpper() ?? "COMPLETADO";
                    int estadoID;
                    
                    if (!estadosDict.TryGetValue(estadoKey, out estadoID))
                    {
                        estadoID = estadoCompletadoID;
                    }

                    var factVenta = new FactVentas
                    {
                        OrdenID = venta.OrdenID,
                        ClienteID = clienteID,
                        ProductoID = productoID,
                        TiempoID = tiempoID,
                        EstadoID = estadoID,
                        Cantidad = venta.Cantidad,
                        PrecioUnitario = venta.Precio,
                        TotalVenta = venta.Cantidad * venta.Precio,
                        FechaCarga = DateTime.Now
                    };

                    factVentas.Add(factVenta);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al transformar venta: {venta.OrdenID}");
                }
            }

            _logger.LogInformation($"=== Resumen de Transformación ===");
            _logger.LogInformation($"  Total transformados: {factVentas.Count}/{ventas.Count}");
            _logger.LogInformation($"  Clientes no encontrados (usando DESCONOCIDO): {clientesNoEncontrados}");
            _logger.LogInformation($"  Productos no encontrados (usando DESCONOCIDO): {productosNoEncontrados}");
            _logger.LogInformation($"  Tiempos nuevos creados: {tiemposNoEncontrados}");

            return factVentas;
        }

        public async Task<bool> ValidateAsync(VentaDTO data)
        {
            return await Task.FromResult(
                !string.IsNullOrWhiteSpace(data.OrdenID) &&
                !string.IsNullOrWhiteSpace(data.ClienteNombre) &&
                !string.IsNullOrWhiteSpace(data.ProductoNombre) &&
                data.Cantidad > 0 &&
                data.Precio > 0
            );
        }
    }
}
