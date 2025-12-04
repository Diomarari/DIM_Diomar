using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Api.DTOs;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DimEstadoController : ControllerBase
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimEstadoController> _logger;

        public DimEstadoController(DataWarehouseContext context, ILogger<DimEstadoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimEstadoResponseDto>>> GetAllEstados()
        {
            try
            {
                var estados = await _context.DimEstados
                    .Select(e => new DimEstadoResponseDto
                    {
                        EstadoID = e.EstadoID,
                        NombreEstado = e.NombreEstado,
                        Descripcion = e.Descripcion,
                        Activo = e.Activo,
                        FechaCreacion = e.FechaCreacion,
                        FechaActualizacion = e.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(estados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DimEstadoResponseDto>> GetEstadoById(int id)
        {
            try
            {
                var estado = await _context.DimEstados
                    .Where(e => e.EstadoID == id)
                    .Select(e => new DimEstadoResponseDto
                    {
                        EstadoID = e.EstadoID,
                        NombreEstado = e.NombreEstado,
                        Descripcion = e.Descripcion,
                        Activo = e.Activo,
                        FechaCreacion = e.FechaCreacion,
                        FechaActualizacion = e.FechaActualizacion
                    })
                    .FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound($"Estado con ID {id} no encontrado");
                }

                return Ok(estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener estado {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/nombre/{nombre}")]
        public async Task<ActionResult<DimEstadoResponseDto>> GetEstadoByNombre(string nombre)
        {
            try
            {
                var estado = await _context.DimEstados
                    .Where(e => e.NombreEstado == nombre.ToUpper())
                    .Select(e => new DimEstadoResponseDto
                    {
                        EstadoID = e.EstadoID,
                        NombreEstado = e.NombreEstado,
                        Descripcion = e.Descripcion,
                        Activo = e.Activo,
                        FechaCreacion = e.FechaCreacion,
                        FechaActualizacion = e.FechaActualizacion
                    })
                    .FirstOrDefaultAsync();

                if (estado == null)
                {
                    return NotFound($"Estado con nombre {nombre} no encontrado");
                }

                return Ok(estado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar estado por nombre {nombre}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/activos")]
        public async Task<ActionResult<IEnumerable<DimEstadoResponseDto>>> GetEstadosActivos()
        {
            try
            {
                var estados = await _context.DimEstados
                    .Where(e => e.Activo)
                    .Select(e => new DimEstadoResponseDto
                    {
                        EstadoID = e.EstadoID,
                        NombreEstado = e.NombreEstado,
                        Descripcion = e.Descripcion,
                        Activo = e.Activo,
                        FechaCreacion = e.FechaCreacion,
                        FechaActualizacion = e.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(estados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estados activos");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
