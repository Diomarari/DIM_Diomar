using SalesAnalyticsETL.Application.Services;

namespace SalesAnalyticsETL.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public Worker(
            ILogger<Worker> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker Service iniciado en: {time}", DateTimeOffset.Now);

            var intervalMinutes = _configuration.GetValue<int>("ETLSettings:IntervalMinutes", 60);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Iniciando ciclo de ETL en: {time}", DateTimeOffset.Now);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var orchestrator = scope.ServiceProvider.GetRequiredService<EnhancedETLOrchestrator>();
                        var result = await orchestrator.ExecuteAsync();

                        if (result.Success)
                        {
                            _logger.LogInformation(
                                "ETL completado exitosamente. " +
                                "Registros cargados: {loaded}/{extracted}. " +
                                "Tiempo: {time}ms",
                                result.LoadedRecords,
                                result.ExtractedRecords,
                                result.TotalTimeMs
                            );
                        }
                        else
                        {
                            _logger.LogError("ETL falló: {error}", result.ErrorMessage);
                        }
                    }

                    _logger.LogInformation(
                        "Próxima ejecución en {minutes} minutos (a las {nextTime})",
                        intervalMinutes,
                        DateTime.Now.AddMinutes(intervalMinutes)
                    );

                    await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error crítico en el Worker Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Worker Service detenido en: {time}", DateTimeOffset.Now);
        }
    }
}