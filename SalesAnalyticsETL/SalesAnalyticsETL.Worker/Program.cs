using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Application.Services;
using SalesAnalyticsETL.Domain.Entities;
using SalesAnalyticsETL.Domain.Interfaces;
using SalesAnalyticsETL.Infrastructure.context;
using SalesAnalyticsETL.Infrastructure.Repositories;
using SalesAnalyticsETL.Infrastructure.Loaders;
using SalesAnalyticsETL.Worker.Jobs;
using Serilog;
using Microsoft.EntityFrameworkCore;

namespace SalesAnalyticsETL.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File("logs/etl-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            try
            {
                Log.Information("Iniciando Worker Service del Sistema ETL de Análisis de Ventas");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "El Worker Service terminó inesperadamente");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    var configuration = hostContext.Configuration;

                    services.AddDbContext<DataWarehouseContext>(options =>
                        options.UseSqlServer(
                            configuration.GetConnectionString("DataWarehouse"),
                            sqlOptions => sqlOptions.EnableRetryOnFailure(
                                maxRetryCount: 5,
                                maxRetryDelay: TimeSpan.FromSeconds(30),
                                errorNumbersToAdd: null
                            )
                        )
                    );

                    services.AddHttpClient("SalesAPI", client =>
                    {
                        client.Timeout = TimeSpan.FromSeconds(30);
                        client.DefaultRequestHeaders.Add("User-Agent", "SalesAnalyticsETL/1.0");
                    });

                    services.AddScoped<IDimClienteLoader, DimClienteLoader>();
                    services.AddScoped<IDimProductoLoader, DimProductoLoader>();
                    services.AddScoped<IDimTiempoLoader, DimTiempoLoader>();
                    services.AddScoped<IDimEstadoLoader, DimEstadoLoader>();

                    services.AddScoped<IExtractor<ClienteDTO>>(sp =>
                        new ClienteCsvExtractor(
                            configuration["DataSources:CustomersCsvPath"] ?? "Data/customers.csv",
                            sp.GetRequiredService<ILogger<ClienteCsvExtractor>>()
                        )
                    );

                    services.AddScoped<IExtractor<ProductoDTO>>(sp =>
                        new ProductExtractorFromCsv(
                            configuration["DataSources:ProductsCsvPath"] ?? "Data/products.csv",
                            sp.GetRequiredService<ILogger<ProductExtractorFromCsv>>()
                        )
                    );

                    services.AddScoped<IExtractor<VentaDTO>>(sp =>
                        new CsvExtractor(
                            configuration["DataSources:CsvPath"] ?? "Data/orders.csv",
                            sp.GetRequiredService<ILogger<CsvExtractor>>()
                        )
                    );

                    services.AddScoped<ITransformer<VentaDTO, FactVentas>, VentasTransformer>();
                    services.AddScoped<ILoader<FactVentas>, DataWarehouseLoader>();

                    services.AddScoped<EnhancedETLOrchestrator>();

                    services.AddHostedService<DimClienteJob>();
                    services.AddHostedService<DimProductoJob>();
                    services.AddHostedService<DimTiempoJob>();
                    services.AddHostedService<DimEstadoJob>();

                    services.AddHostedService<Worker>();
                });
    }
}