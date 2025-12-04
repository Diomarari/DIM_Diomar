using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Infrastructure.Loaders
{
    public class DimTiempoLoader : IDimTiempoLoader
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimTiempoLoader> _logger;

        public DimTiempoLoader(DataWarehouseContext context, ILogger<DimTiempoLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> LoadDimensionForDateAsync(DateTime fecha)
        {
            try
            {
                var fechaSoloFecha = fecha.Date;

                var tiempoExistente = await _context.DimTiempos
                    .FirstOrDefaultAsync(t => t.Fecha == fechaSoloFecha);

                if (tiempoExistente != null)
                {
                    return tiempoExistente.TiempoID;
                }

                var nuevoTiempo = new DimTiempo
                {
                    Fecha = fechaSoloFecha,
                    Anio = fecha.Year,
                    Mes = fecha.Month,
                    Dia = fecha.Day,
                    Trimestre = GetTrimestre(fecha.Month),
                    NombreMes = GetNombreMes(fecha.Month),
                    DiaSemana = GetNombreDia(fecha.DayOfWeek),
                    EsFinDeSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday,
                    EsFeriado = false 
                };

                _context.DimTiempos.Add(nuevoTiempo);
                await _context.SaveChangesAsync();

                return nuevoTiempo.TiempoID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dimensión DimTiempo para fecha {fecha}", fecha);
                throw;
            }
        }

        public async Task<int> LoadDimensionForDateRangeAsync(IEnumerable<DateTime> fechas)
        {
            var loadedCount = 0;

            try
            {
                _logger.LogInformation("Iniciando carga optimizada de dimensión DimTiempo...");

                var fechasUnicas = fechas
                    .Select(f => f.Date)
                    .Distinct()
                    .OrderBy(f => f)
                    .ToList();

                var fechasExistentes = await _context.DimTiempos
                    .AsNoTracking()
                    .Select(t => t.Fecha)
                    .ToHashSetAsync();

                _logger.LogInformation("Fechas existentes en BD: {count}", fechasExistentes.Count);

                var fechasNuevas = fechasUnicas
                    .Where(f => !fechasExistentes.Contains(f))
                    .ToList();

                if (!fechasNuevas.Any())
                {
                    _logger.LogInformation("No hay fechas nuevas para cargar");
                    return 0;
                }

                var nuevosTiempos = fechasNuevas.Select(fecha => new DimTiempo
                {
                    Fecha = fecha,
                    Anio = fecha.Year,
                    Mes = fecha.Month,
                    Dia = fecha.Day,
                    Trimestre = GetTrimestre(fecha.Month),
                    NombreMes = GetNombreMes(fecha.Month),
                    DiaSemana = GetNombreDia(fecha.DayOfWeek),
                    EsFinDeSemana = fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday,
                    EsFeriado = EsFeriado(fecha)
                }).ToList();

                await _context.DimTiempos.AddRangeAsync(nuevosTiempos);
                await _context.SaveChangesAsync();

                loadedCount = nuevosTiempos.Count;
                
                _logger.LogInformation(
                    "? Dimensión DimTiempo cargada: {nuevos} fechas nuevas en un solo lote",
                    loadedCount);

                return loadedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dimensión DimTiempo");
                throw;
            }
        }

        public async Task<int?> GetTiempoIDByDateAsync(DateTime fecha)
        {
            var fechaSoloFecha = fecha.Date;
            
            var tiempo = await _context.DimTiempos
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Fecha == fechaSoloFecha);

            return tiempo?.TiempoID;
        }

        public async Task<int> PreloadYearAsync(int year)
        {
            var loadedCount = 0;
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            _logger.LogInformation("Pre-cargando dimensión DimTiempo para el año {year}...", year);

            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var exists = await _context.DimTiempos.AnyAsync(t => t.Fecha == date);
                
                if (!exists)
                {
                    var nuevoTiempo = new DimTiempo
                    {
                        Fecha = date,
                        Anio = date.Year,
                        Mes = date.Month,
                        Dia = date.Day,
                        Trimestre = GetTrimestre(date.Month),
                        NombreMes = GetNombreMes(date.Month),
                        DiaSemana = GetNombreDia(date.DayOfWeek),
                        EsFinDeSemana = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday,
                        EsFeriado = EsFeriado(date)
                    };

                    _context.DimTiempos.Add(nuevoTiempo);
                    loadedCount++;

                    if (loadedCount % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
            }

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("? Pre-cargadas {count} fechas para el año {year}", loadedCount, year);
            
            return loadedCount;
        }

        #region Métodos auxiliares

        private int GetTrimestre(int mes)
        {
            return (mes - 1) / 3 + 1;
        }

        private string GetNombreMes(int mes)
        {
            return mes switch
            {
                1 => "Enero",
                2 => "Febrero",
                3 => "Marzo",
                4 => "Abril",
                5 => "Mayo",
                6 => "Junio",
                7 => "Julio",
                8 => "Agosto",
                9 => "Septiembre",
                10 => "Octubre",
                11 => "Noviembre",
                12 => "Diciembre",
                _ => "Desconocido"
            };
        }

        private string GetNombreDia(DayOfWeek dia)
        {
            return dia switch
            {
                DayOfWeek.Monday => "Lunes",
                DayOfWeek.Tuesday => "Martes",
                DayOfWeek.Wednesday => "Miércoles",
                DayOfWeek.Thursday => "Jueves",
                DayOfWeek.Friday => "Viernes",
                DayOfWeek.Saturday => "Sábado",
                DayOfWeek.Sunday => "Domingo",
                _ => "Desconocido"
            };
        }

        private bool EsFeriado(DateTime fecha)
        {
            
            if (fecha.Month == 1 && fecha.Day == 1) return true;  
            if (fecha.Month == 12 && fecha.Day == 25) return true; 
            
            return false;
        }

        #endregion
    }
}
