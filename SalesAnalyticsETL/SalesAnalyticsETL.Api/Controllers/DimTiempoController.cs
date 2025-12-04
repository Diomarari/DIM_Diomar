using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Api.DTOs;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DimTiempoController : ControllerBase
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimTiempoController> _logger;

        public DimTiempoController(DataWarehouseContext context, ILogger<DimTiempoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimTiempoResponseDto>>> GetAllTiempos(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            try
            {
                var query = _context.DimTiempos.AsQueryable();

                if (year.HasValue)
                    query = query.Where(t => t.Anio == year.Value);

                if (month.HasValue)
                    query = query.Where(t => t.Mes == month.Value);

                var tiempos = await query
                    .Select(t => new DimTiempoResponseDto
                    {
                        TiempoID = t.TiempoID,
                        Fecha = t.Fecha,
                        Anio = t.Anio,
                        Mes = t.Mes,
                        Dia = t.Dia,
                        DiaSemana = t.DiaSemana,
                        NombreMes = t.NombreMes,
                        Trimestre = t.Trimestre,
                        EsFinDeSemana = t.EsFinDeSemana,
                        EsFeriado = t.EsFeriado
                    })
                    .OrderBy(t => t.Fecha)
                    .ToListAsync();

                return Ok(tiempos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener registros de tiempo");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DimTiempoResponseDto>> GetTiempoById(int id)
        {
            try
            {
                var tiempo = await _context.DimTiempos
                    .Where(t => t.TiempoID == id)
                    .Select(t => new DimTiempoResponseDto
                    {
                        TiempoID = t.TiempoID,
                        Fecha = t.Fecha,
                        Anio = t.Anio,
                        Mes = t.Mes,
                        Dia = t.Dia,
                        DiaSemana = t.DiaSemana,
                        NombreMes = t.NombreMes,
                        Trimestre = t.Trimestre,
                        EsFinDeSemana = t.EsFinDeSemana,
                        EsFeriado = t.EsFeriado
                    })
                    .FirstOrDefaultAsync();

                if (tiempo == null)
                {
                    return NotFound($"Registro de tiempo con ID {id} no encontrado");
                }

                return Ok(tiempo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tiempo {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/fecha/{fecha}")]
        public async Task<ActionResult<DimTiempoResponseDto>> GetTiempoByFecha(DateTime fecha)
        {
            try
            {
                var tiempo = await _context.DimTiempos
                    .Where(t => t.Fecha.Date == fecha.Date)
                    .Select(t => new DimTiempoResponseDto
                    {
                        TiempoID = t.TiempoID,
                        Fecha = t.Fecha,
                        Anio = t.Anio,
                        Mes = t.Mes,
                        Dia = t.Dia,
                        DiaSemana = t.DiaSemana,
                        NombreMes = t.NombreMes,
                        Trimestre = t.Trimestre,
                        EsFinDeSemana = t.EsFinDeSemana,
                        EsFeriado = t.EsFeriado
                    })
                    .FirstOrDefaultAsync();

                if (tiempo == null)
                {
                    return NotFound($"Registro de tiempo para la fecha {fecha:yyyy-MM-dd} no encontrado");
                }

                return Ok(tiempo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar tiempo por fecha {fecha}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/rango")]
        public async Task<ActionResult<IEnumerable<DimTiempoResponseDto>>> GetTiemposByRango(
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var tiempos = await _context.DimTiempos
                    .Where(t => t.Fecha.Date >= fechaInicio.Date && t.Fecha.Date <= fechaFin.Date)
                    .Select(t => new DimTiempoResponseDto
                    {
                        TiempoID = t.TiempoID,
                        Fecha = t.Fecha,
                        Anio = t.Anio,
                        Mes = t.Mes,
                        Dia = t.Dia,
                        DiaSemana = t.DiaSemana,
                        NombreMes = t.NombreMes,
                        Trimestre = t.Trimestre,
                        EsFinDeSemana = t.EsFinDeSemana,
                        EsFeriado = t.EsFeriado
                    })
                    .OrderBy(t => t.Fecha)
                    .ToListAsync();

                return Ok(tiempos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener tiempos por rango");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/fines-de-semana")]
        public async Task<ActionResult<IEnumerable<DimTiempoResponseDto>>> GetFinesDeSemana(
            [FromQuery] int? year = null,
            [FromQuery] int? month = null)
        {
            try
            {
                var query = _context.DimTiempos.Where(t => t.EsFinDeSemana);

                if (year.HasValue)
                    query = query.Where(t => t.Anio == year.Value);

                if (month.HasValue)
                    query = query.Where(t => t.Mes == month.Value);

                var tiempos = await query
                    .Select(t => new DimTiempoResponseDto
                    {
                        TiempoID = t.TiempoID,
                        Fecha = t.Fecha,
                        Anio = t.Anio,
                        Mes = t.Mes,
                        Dia = t.Dia,
                        DiaSemana = t.DiaSemana,
                        NombreMes = t.NombreMes,
                        Trimestre = t.Trimestre,
                        EsFinDeSemana = t.EsFinDeSemana,
                        EsFeriado = t.EsFeriado
                    })
                    .OrderBy(t => t.Fecha)
                    .ToListAsync();

                return Ok(tiempos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener fines de semana");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
