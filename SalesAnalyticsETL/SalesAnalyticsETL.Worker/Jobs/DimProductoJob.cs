using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;

namespace SalesAnalyticsETL.Worker.Jobs
{
    public class DimProductoJob : BackgroundService
    {
        private readonly ILogger<DimProductoJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _intervalMinutes;
        private readonly int _startupDelaySeconds;
        private readonly bool _enabled;

        public DimProductoJob(
            ILogger<DimProductoJob> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _enabled = _configuration.GetValue<bool>("DimensionJobs:DimProducto:Enabled", true);
            _intervalMinutes = _configuration.GetValue<int>("DimensionJobs:DimProducto:IntervalMinutes", 30);
            _startupDelaySeconds = _configuration.GetValue<int>("DimensionJobs:DimProducto:StartupDelaySeconds", 10);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("DimProductoJob deshabilitado");
                return;
            }

            _logger.LogInformation("DimProductoJob iniciado - Intervalo: {minutes} min", _intervalMinutes);

            await Task.Delay(TimeSpan.FromSeconds(_startupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("[DimProducto] Iniciando carga...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var extractor = scope.ServiceProvider.GetRequiredService<IExtractor<ProductoDTO>>();
                        var loader = scope.ServiceProvider.GetRequiredService<IDimProductoLoader>();

                        var data = await extractor.ExtractAsync();
                        var loaded = await loader.LoadDimensionAsync(data);

                        _logger.LogInformation("[DimProducto] Completado: {count} registros", loaded);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DimProducto] Error en job");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
