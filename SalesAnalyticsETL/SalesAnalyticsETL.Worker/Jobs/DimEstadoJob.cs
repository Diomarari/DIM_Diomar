using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;

namespace SalesAnalyticsETL.Worker.Jobs
{
    public class DimEstadoJob : BackgroundService
    {
        private readonly ILogger<DimEstadoJob> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly int _intervalMinutes;
        private readonly int _startupDelaySeconds;
        private readonly bool _enabled;

        public DimEstadoJob(
            ILogger<DimEstadoJob> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            _enabled = _configuration.GetValue<bool>("DimensionJobs:DimEstado:Enabled", true);
            _intervalMinutes = _configuration.GetValue<int>("DimensionJobs:DimEstado:IntervalMinutes", 60);
            _startupDelaySeconds = _configuration.GetValue<int>("DimensionJobs:DimEstado:StartupDelaySeconds", 20);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("DimEstadoJob deshabilitado");
                return;
            }

            _logger.LogInformation("DimEstadoJob iniciado - Intervalo: {minutes} min", _intervalMinutes);

            await Task.Delay(TimeSpan.FromSeconds(_startupDelaySeconds), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("[DimEstado] Iniciando carga...");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var extractor = scope.ServiceProvider.GetRequiredService<IExtractor<VentaDTO>>();
                        var loader = scope.ServiceProvider.GetRequiredService<IDimEstadoLoader>();

                        var ventas = await extractor.ExtractAsync();
                        var estados = ventas.Select(v => v.Estado).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct();

                        var loaded = 0;
                        foreach (var estado in estados)
                        {
                            await loader.GetOrCreateEstadoByNameAsync(estado);
                            loaded++;
                        }

                        _logger.LogInformation("[DimEstado] Completado: {count} estados", loaded);
                    }

                    await Task.Delay(TimeSpan.FromMinutes(_intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[DimEstado] Error en job");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }
    }
}
