using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Api.DTOs;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DimClienteController : ControllerBase
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimClienteController> _logger;

        public DimClienteController(DataWarehouseContext context, ILogger<DimClienteController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimClienteResponseDto>>> GetAllClientes()
        {
            try
            {
                var clientes = await _context.DimClientes
                    .Select(c => new DimClienteResponseDto
                    {
                        ClienteID = c.ClienteID,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Ciudad = c.Ciudad,
                        Pais = c.Pais,
                        FechaCreacion = c.FechaCreacion,
                        FechaActualizacion = c.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener clientes");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DimClienteResponseDto>> GetClienteById(int id)
        {
            try
            {
                var cliente = await _context.DimClientes
                    .Where(c => c.ClienteID == id)
                    .Select(c => new DimClienteResponseDto
                    {
                        ClienteID = c.ClienteID,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Ciudad = c.Ciudad,
                        Pais = c.Pais,
                        FechaCreacion = c.FechaCreacion,
                        FechaActualizacion = c.FechaActualizacion
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound($"Cliente con ID {id} no encontrado");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener cliente {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/email/{email}")]
        public async Task<ActionResult<DimClienteResponseDto>> GetClienteByEmail(string email)
        {
            try
            {
                var cliente = await _context.DimClientes
                    .Where(c => c.Email == email)
                    .Select(c => new DimClienteResponseDto
                    {
                        ClienteID = c.ClienteID,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Ciudad = c.Ciudad,
                        Pais = c.Pais,
                        FechaCreacion = c.FechaCreacion,
                        FechaActualizacion = c.FechaActualizacion
                    })
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    return NotFound($"Cliente con email {email} no encontrado");
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar cliente por email {email}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/pais/{pais}")]
        public async Task<ActionResult<IEnumerable<DimClienteResponseDto>>> GetClientesByPais(string pais)
        {
            try
            {
                var clientes = await _context.DimClientes
                    .Where(c => c.Pais == pais)
                    .Select(c => new DimClienteResponseDto
                    {
                        ClienteID = c.ClienteID,
                        Nombre = c.Nombre,
                        Apellido = c.Apellido,
                        Email = c.Email,
                        Telefono = c.Telefono,
                        Ciudad = c.Ciudad,
                        Pais = c.Pais,
                        FechaCreacion = c.FechaCreacion,
                        FechaActualizacion = c.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener clientes por país {pais}");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
