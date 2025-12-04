using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Infrastructure.Loaders
{
    public class DimProductoLoader : IDimProductoLoader
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimProductoLoader> _logger;

        public DimProductoLoader(DataWarehouseContext context, ILogger<DimProductoLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> LoadDimensionAsync<TProductoDTO>(IEnumerable<TProductoDTO> productos) where TProductoDTO : class
        {
            var loadedCount = 0;
            var updatedCount = 0;

            try
            {
                _logger.LogInformation("Iniciando carga optimizada de dimensión DimProducto...");

                var productoDtoList = productos.Cast<ProductoDTO>().ToList();

                var productosExistentes = await _context.DimProductos
                    .AsNoTracking()
                    .ToListAsync();

                var productosExistentesDict = productosExistentes
                    .Where(p => !string.IsNullOrWhiteSpace(p.NombreProducto))
                    .GroupBy(p => p.NombreProducto.ToLower())
                    .ToDictionary(g => g.Key, g => g.First());

                _logger.LogInformation("Productos existentes en BD: {total} ({unicos} únicos)", 
                    productosExistentes.Count, productosExistentesDict.Count);

                if (productosExistentes.Count > productosExistentesDict.Count)
                {
                    _logger.LogWarning("?? Se encontraron {duplicados} productos duplicados en la BD", 
                        productosExistentes.Count - productosExistentesDict.Count);
                }

                var nuevosProductos = new List<DimProducto>();
                var productosParaActualizar = new List<(int ProductoID, ProductoDTO Dto)>();

                foreach (var productoDto in productoDtoList)
                {
                    var nombreLower = productoDto.NombreProducto?.ToLower() ?? "";

                    if (string.IsNullOrWhiteSpace(nombreLower))
                        continue;

                    if (productosExistentesDict.TryGetValue(nombreLower, out var productoExistente))
                    {
                        productosParaActualizar.Add((productoExistente.ProductoID, productoDto));
                        updatedCount++;
                    }
                    else
                    {
                        var nuevoProducto = new DimProducto
                        {
                            NombreProducto = productoDto.NombreProducto ?? "DESCONOCIDO",
                            Categoria = productoDto.Categoria ?? "SIN CATEGORÍA",
                            PrecioBase = productoDto.PrecioBase,
                            Stock = productoDto.Stock,
                            FechaCreacion = DateTime.UtcNow
                        };

                        nuevosProductos.Add(nuevoProducto);
                        productosExistentesDict.Add(nombreLower, nuevoProducto); 
                        loadedCount++;
                    }
                }

                if (productosParaActualizar.Any())
                {
                    _logger.LogInformation("Actualizando {count} productos existentes", productosParaActualizar.Count);

                    foreach (var (productoID, dto) in productosParaActualizar)
                    {
                        await _context.DimProductos
                            .Where(p => p.ProductoID == productoID)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(p => p.Categoria, dto.Categoria ?? "SIN CATEGORÍA")
                                .SetProperty(p => p.PrecioBase, dto.PrecioBase > 0 ? dto.PrecioBase : 0)
                                .SetProperty(p => p.Stock, dto.Stock)
                                .SetProperty(p => p.FechaActualizacion, DateTime.UtcNow)
                            );
                    }
                }

                if (nuevosProductos.Any())
                {
                    await _context.DimProductos.AddRangeAsync(nuevosProductos);
                    await _context.SaveChangesAsync();
                }
                
                _logger.LogInformation(
                    "? Dimensión DimProducto procesada: {nuevos} nuevos, {actualizados} actualizados",
                    loadedCount, updatedCount);

                return loadedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dimensión DimProducto");
                throw;
            }
        }

        public async Task<int?> GetProductoIDByNameAsync(string nombreProducto)
        {
            if (string.IsNullOrWhiteSpace(nombreProducto))
                return null;

            var producto = await _context.DimProductos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.NombreProducto == nombreProducto);

            return producto?.ProductoID;
        }

        public async Task<int> GetOrCreateUnknownProductoAsync()
        {
            var unknownName = "DESCONOCIDO";
            var producto = await _context.DimProductos
                .FirstOrDefaultAsync(p => p.NombreProducto == unknownName);

            if (producto == null)
            {
                producto = new DimProducto
                {
                    NombreProducto = unknownName,
                    Categoria = "SIN CATEGORÍA",
                    PrecioBase = 0,
                    Stock = 0,
                    FechaCreacion = DateTime.UtcNow
                };

                _context.DimProductos.Add(producto);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Producto DESCONOCIDO creado con ID: {id}", producto.ProductoID);
            }

            return producto.ProductoID;
        }
    }
}
