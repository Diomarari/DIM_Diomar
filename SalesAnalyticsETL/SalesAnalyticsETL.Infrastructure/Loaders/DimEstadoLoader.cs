using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Infrastructure.Loaders
{
    public class DimEstadoLoader : IDimEstadoLoader
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimEstadoLoader> _logger;

        public DimEstadoLoader(DataWarehouseContext context, ILogger<DimEstadoLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> GetOrCreateEstadoByNameAsync(string nombreEstado)
        {
            try
            {
                nombreEstado = nombreEstado.ToUpper().Trim();

                var estado = await _context.DimEstados
                    .FirstOrDefaultAsync(e => e.NombreEstado == nombreEstado);

                if (estado != null)
                {
                    return estado.EstadoID;
                }

                var nuevoEstado = new DimEstado
                {
                    NombreEstado = nombreEstado,
                    Descripcion = $"Estado {nombreEstado}",
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                _context.DimEstados.Add(nuevoEstado);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Estado creado: {nombreEstado} (ID: {nuevoEstado.EstadoID})");

                return nuevoEstado.EstadoID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener o crear estado: {nombreEstado}");
                throw;
            }
        }

        public async Task<int?> GetEstadoIDByNameAsync(string nombreEstado)
        {
            try
            {
                nombreEstado = nombreEstado.ToUpper().Trim();

                var estado = await _context.DimEstados
                    .FirstOrDefaultAsync(e => e.NombreEstado == nombreEstado);

                return estado?.EstadoID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar estado: {nombreEstado}");
                return null;
            }
        }
    }
}
