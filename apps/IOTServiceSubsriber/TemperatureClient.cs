using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace IOTServiceSubsriber
{
    public class TemperatureClient : ITemperatureClient
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TemperatureClient> _logger;

        public TemperatureClient(IHttpClientFactory httpClientFactory, ILogger<TemperatureClient> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<TemperatureDataDto> GetTemperatureAsync(int sensorId)
        {
            var client = _httpClientFactory.CreateClient("temperature-client");
            using var response = await client.GetAsync($"/temperature/{sensorId}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Failed to get last temperature for {sensorId}: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadFromJsonAsync<TemperatureDataDto>();
        }
    }
}
