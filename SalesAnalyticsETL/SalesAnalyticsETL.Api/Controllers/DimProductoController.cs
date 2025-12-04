using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesAnalyticsETL.Api.DTOs;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DimProductoController : ControllerBase
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimProductoController> _logger;

        public DimProductoController(DataWarehouseContext context, ILogger<DimProductoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DimProductoResponseDto>>> GetAllProductos()
        {
            try
            {
                var productos = await _context.DimProductos
                    .Select(p => new DimProductoResponseDto
                    {
                        ProductoID = p.ProductoID,
                        NombreProducto = p.NombreProducto,
                        Categoria = p.Categoria,
                        PrecioBase = p.PrecioBase,
                        Stock = p.Stock,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DimProductoResponseDto>> GetProductoById(int id)
        {
            try
            {
                var producto = await _context.DimProductos
                    .Where(p => p.ProductoID == id)
                    .Select(p => new DimProductoResponseDto
                    {
                        ProductoID = p.ProductoID,
                        NombreProducto = p.NombreProducto,
                        Categoria = p.Categoria,
                        PrecioBase = p.PrecioBase,
                        Stock = p.Stock,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                {
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener producto {id}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/nombre/{nombre}")]
        public async Task<ActionResult<IEnumerable<DimProductoResponseDto>>> GetProductosByNombre(string nombre)
        {
            try
            {
                var productos = await _context.DimProductos
                    .Where(p => p.NombreProducto.Contains(nombre))
                    .Select(p => new DimProductoResponseDto
                    {
                        ProductoID = p.ProductoID,
                        NombreProducto = p.NombreProducto,
                        Categoria = p.Categoria,
                        PrecioBase = p.PrecioBase,
                        Stock = p.Stock,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al buscar productos por nombre {nombre}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/categoria/{categoria}")]
        public async Task<ActionResult<IEnumerable<DimProductoResponseDto>>> GetProductosByCategoria(string categoria)
        {
            try
            {
                var productos = await _context.DimProductos
                    .Where(p => p.Categoria == categoria)
                    .Select(p => new DimProductoResponseDto
                    {
                        ProductoID = p.ProductoID,
                        NombreProducto = p.NombreProducto,
                        Categoria = p.Categoria,
                        PrecioBase = p.PrecioBase,
                        Stock = p.Stock,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener productos por categoría {categoria}");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        [HttpGet("search/precio")]
        public async Task<ActionResult<IEnumerable<DimProductoResponseDto>>> GetProductosByPrecio(
            [FromQuery] decimal? minPrecio, 
            [FromQuery] decimal? maxPrecio)
        {
            try
            {
                var query = _context.DimProductos.AsQueryable();

                if (minPrecio.HasValue)
                    query = query.Where(p => p.PrecioBase >= minPrecio.Value);

                if (maxPrecio.HasValue)
                    query = query.Where(p => p.PrecioBase <= maxPrecio.Value);

                var productos = await query
                    .Select(p => new DimProductoResponseDto
                    {
                        ProductoID = p.ProductoID,
                        NombreProducto = p.NombreProducto,
                        Categoria = p.Categoria,
                        PrecioBase = p.PrecioBase,
                        Stock = p.Stock,
                        FechaCreacion = p.FechaCreacion,
                        FechaActualizacion = p.FechaActualizacion
                    })
                    .ToListAsync();

                return Ok(productos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar productos por precio");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }
}
