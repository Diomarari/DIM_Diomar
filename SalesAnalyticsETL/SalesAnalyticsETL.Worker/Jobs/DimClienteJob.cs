using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;

namespace SalesAnalyticsETL.Worker.Jobs
{
    public class DimClienteJob : BackgroundService
    {
        private readonly ILogger<DimClienteJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _intervalMinutes;
        private readonly int _startupDelaySeconds;
        private readonly bool _enabled;

        public DimClienteJob(
            ILogger<DimClienteJob> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _enabled = _configuration.GetValue<bool>("DimensionJobs:DimCliente:Enabled", true);
            _intervalMinutes = _configuration.GetValue<int>("DimensionJobs:DimCliente:IntervalMinutes", 30);
            _startupDelaySeconds = _configuration.GetValue<int>("DimensionJobs:DimCliente:StartupDelaySeconds", 5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("DimClienteJob deshabilitado");
                return;
            }

            _logger.LogInformation("DimClienteJob iniciado - Intervalo: {minutes} min", _intervalMinutes);

            await Task.Delay(TimeSpan.FromSeconds(_startupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("[DimCliente] Iniciando carga...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var extractor = scope.ServiceProvider.GetRequiredService<IExtractor<ClienteDTO>>();
                        var loader = scope.ServiceProvider.GetRequiredService<IDimClienteLoader>();

                        var data = await extractor.ExtractAsync();
                        var loaded = await loader.LoadDimensionAsync(data);

                        _logger.LogInformation("[DimCliente] Completado: {count} registros", loaded);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DimCliente] Error en job");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
