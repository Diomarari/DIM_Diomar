using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SalesAnalyticsETL.Application.DTOs;
using SalesAnalyticsETL.Domain.Interfaces;
using System.Net.Http.Json;

namespace SalesAnalyticsETL.Infrastructure.Repositories
{
    public class ApiExtractor : IExtractor<VentaDTO>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiExtractor> _logger;

        public ApiExtractor(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<ApiExtractor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<VentaDTO>> ExtractAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("SalesAPI");
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000";
                var endpoint = $"{apiUrl}/api/ventas/recientes";

                _logger.LogInformation($"Llamando a API: {endpoint}");

                var response = await client.GetAsync(endpoint);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"API retornó status code: {response.StatusCode}");
                    return new List<VentaDTO>();
                }

                var ventas = await response.Content.ReadFromJsonAsync<List<VentaDTO>>()
                    ?? new List<VentaDTO>();

                _logger.LogInformation($"API: {ventas.Count} registros extraídos exitosamente");
                return ventas;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "Error de conexión con la API. Continuando con otras fuentes...");
                return new List<VentaDTO>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al extraer datos de la API");
                return new List<VentaDTO>();
            }
        }

        public string GetSourceName() => "REST API (External)";
    }
}