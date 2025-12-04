using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;

namespace SalesAnalyticsETL.Infrastructure.Loaders
{
    public class DimClienteLoader : IDimClienteLoader
    {
        private readonly DataWarehouseContext _context;
        private readonly ILogger<DimClienteLoader> _logger;
        private const int BATCH_SIZE = 1000;
        private const int QUERY_PAGE_SIZE = 5000;

        public DimClienteLoader(DataWarehouseContext context, ILogger<DimClienteLoader> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> LoadDimensionAsync<TClienteDTO>(IEnumerable<TClienteDTO> clientes) where TClienteDTO : class
        {
            var loadedCount = 0;
            var updatedCount = 0;

            try
            {
                _logger.LogInformation("Iniciando carga optimizada de dimensión DimCliente con paginación...");

                var clienteDtoList = clientes.Cast<ClienteDTO>().ToList();
                
                if (!clienteDtoList.Any())
                {
                    _logger.LogWarning("No hay clientes para procesar");
                    return 0;
                }

                _logger.LogInformation("Total de clientes a procesar: {count}", clienteDtoList.Count);

                var clientesExistentesDict = await _context.DimClientes
                    .AsNoTracking()
                    .ToDictionaryAsync(
                        c => c.Email.ToLower(),
                        c => new { c.ClienteID, c.Email, c.Nombre, c.Apellido, c.Telefono, c.Ciudad, c.Pais }
                    );

                _logger.LogInformation("Clientes existentes cargados: {count}", clientesExistentesDict.Count);

                var nuevosClientes = new List<DimCliente>();
                var clientesParaActualizar = new List<(int ClienteID, ClienteDTO Dto)>();

                foreach (var clienteDto in clienteDtoList)
                {
                    var emailLower = clienteDto.Email?.ToLower() ?? "";

                    if (string.IsNullOrWhiteSpace(emailLower))
                    {
                        emailLower = $"sin-email-{Guid.NewGuid()}@sistema.local";
                    }

                    if (clientesExistentesDict.TryGetValue(emailLower, out var clienteExistente))
                    {
                        clientesParaActualizar.Add((clienteExistente.ClienteID, clienteDto));
                        updatedCount++;
                    }
                    else
                    {
                        var nuevoCliente = new DimCliente
                        {
                            Nombre = clienteDto.Nombre ?? "DESCONOCIDO",
                            Apellido = clienteDto.Apellido ?? "",
                            Email = emailLower,
                            Telefono = clienteDto.Telefono ?? "",
                            Ciudad = clienteDto.Ciudad ?? "",
                            Pais = clienteDto.Pais ?? "",
                            FechaCreacion = DateTime.UtcNow
                        };

                        nuevosClientes.Add(nuevoCliente);
                        loadedCount++;
                    }
                }

                _logger.LogInformation("Guardando cambios: {nuevos} nuevos, {actualizados} actualizaciones", 
                    nuevosClientes.Count, clientesParaActualizar.Count);

                if (clientesParaActualizar.Any())
                {
                    _logger.LogInformation("Actualizando {count} clientes existentes", clientesParaActualizar.Count);

                    foreach (var (clienteID, dto) in clientesParaActualizar)
                    {
                        await _context.DimClientes
                            .Where(c => c.ClienteID == clienteID)
                            .ExecuteUpdateAsync(setters => setters
                                .SetProperty(c => c.Nombre, dto.Nombre ?? "DESCONOCIDO")
                                .SetProperty(c => c.Apellido, dto.Apellido ?? "")
                                .SetProperty(c => c.Telefono, dto.Telefono ?? "")
                                .SetProperty(c => c.Ciudad, dto.Ciudad ?? "")
                                .SetProperty(c => c.Pais, dto.Pais ?? "")
                                .SetProperty(c => c.FechaActualizacion, DateTime.UtcNow)
                            );
                    }

                    _logger.LogInformation("Clientes actualizados exitosamente");
                }

                if (nuevosClientes.Any())
                {
                    var insertBatches = (int)Math.Ceiling((double)nuevosClientes.Count / BATCH_SIZE);
                    _logger.LogInformation("Insertando en {batches} lotes de {size}", insertBatches, BATCH_SIZE);

                    for (int i = 0; i < nuevosClientes.Count; i += BATCH_SIZE)
                    {
                        var batch = nuevosClientes.Skip(i).Take(BATCH_SIZE).ToList();
                        
                        await _context.DimClientes.AddRangeAsync(batch);
                        await _context.SaveChangesAsync();
                        _context.ChangeTracker.Clear();

                        _logger.LogInformation("  Lote {current}/{total} insertado", (i / BATCH_SIZE) + 1, insertBatches);
                    }
                }
                
                _logger.LogInformation(
                    "? Dimensión DimCliente procesada: {nuevos} nuevos, {actualizados} actualizados",
                    loadedCount, updatedCount);

                return loadedCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dimensión DimCliente");
                throw;
            }
        }

        public async Task<int?> GetClienteIDByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            try
            {
                var clienteID = await _context.DimClientes
                    .AsNoTracking()
                    .Where(c => c.Email == email)
                    .Select(c => c.ClienteID)
                    .FirstOrDefaultAsync();

                return clienteID > 0 ? clienteID : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar cliente por email: {email}", email);
                return null;
            }
        }
        public async Task<int> GetOrCreateUnknownClienteAsync()
        {
            var unknownEmail = "desconocido@sistema.local";
            
            try
            {
                var cliente = await _context.DimClientes
                    .Where(c => c.Email == unknownEmail)
                    .FirstOrDefaultAsync();

                if (cliente == null)
                {
                    cliente = new DimCliente
                    {
                        Nombre = "DESCONOCIDO",
                        Apellido = "",
                        Email = unknownEmail,
                        Telefono = "",
                        Ciudad = "",
                        Pais = "",
                        FechaCreacion = DateTime.UtcNow
                    };

                    _context.DimClientes.Add(cliente);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Cliente DESCONOCIDO creado con ID: {id}", cliente.ClienteID);
                }

                return cliente.ClienteID;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener/crear cliente DESCONOCIDO");
                throw;
            }
        }
    }
}
