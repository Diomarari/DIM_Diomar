using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;

namespace SalesAnalyticsETL.Worker.Jobs
{
    public class DimTiempoJob : BackgroundService
    {
        private readonly ILogger<DimTiempoJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _intervalMinutes;
        private readonly int _startupDelaySeconds;
        private readonly bool _enabled;

        public DimTiempoJob(
            ILogger<DimTiempoJob> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _enabled = _configuration.GetValue<bool>("DimensionJobs:DimTiempo:Enabled", true);
            _intervalMinutes = _configuration.GetValue<int>("DimensionJobs:DimTiempo:IntervalMinutes", 60);
            _startupDelaySeconds = _configuration.GetValue<int>("DimensionJobs:DimTiempo:StartupDelaySeconds", 15);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("DimTiempoJob deshabilitado");
                return;
            }

            _logger.LogInformation("DimTiempoJob iniciado - Intervalo: {minutes} min", _intervalMinutes);

            await Task.Delay(TimeSpan.FromSeconds(_startupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("[DimTiempo] Iniciando carga...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var extractor = scope.ServiceProvider.GetRequiredService<IExtractor<VentaDTO>>();
                        var loader = scope.ServiceProvider.GetRequiredService<IDimTiempoLoader>();

                        var ventas = await extractor.ExtractAsync();
                        var fechas = ventas.Select(v => v.FechaVenta);
                        var loaded = await loader.LoadDimensionForDateRangeAsync(fechas);

                        _logger.LogInformation("[DimTiempo] Completado: {count} fechas", loaded);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DimTiempo] Error en job");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
