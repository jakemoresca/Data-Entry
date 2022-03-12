using DataEntry.Client.Models;
using DataEntry.Client.Options;
using Microsoft.Extensions.Options;

namespace DataEntry.Client.Services
{
    public class WeatherForecastService
    {
        private readonly HttpClient _httpClient;
        private readonly IOptions<ApiOptions> _options;

        public WeatherForecastService(IHttpClientFactory httpClientFactory, IOptions<ApiOptions> options)
        {
            _httpClient = httpClientFactory.CreateClient();
            _options = options;
        }

        public async Task<WeatherForecast[]?> GetForecastAsync(DateTime startDate)
        {
            var baseUrl = new Uri(_options.Value.URL);
            var url = new Uri(baseUrl, $"/WeatherForecast/{startDate.ToString("yyyy-MM-dd")}");

            return await _httpClient.GetFromJsonAsync<WeatherForecast[]>(url);
        }
    }
}