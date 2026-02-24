using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ProjekatWebKulturniDogadjaji.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public WeatherService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["WeatherSettings:ApiKey"];
            _baseUrl = configuration["WeatherSettings:BaseUrl"];
        }

        public async Task<WeatherInfo> GetWeatherAsync(string city)
        {
            if (string.IsNullOrWhiteSpace(city)) return null;

            var url = $"{_baseUrl}?q={city}&appid={_apiKey}&units=metric";

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    
                    return null;
                }

                var data = await response.Content.ReadFromJsonAsync<OpenWeatherResponse>();
                if (data == null || data.Weather == null || data.Weather.Length == 0) return null;

                return new WeatherInfo
                {
                   
                    Temperature = data.Main.Temp,
                    Description = data.Weather[0].Description
                };
            }
            catch
            {
                return null;
            }
        }

        private class OpenWeatherResponse
        {
            public MainInfo Main { get; set; }
            public WeatherDescription[] Weather { get; set; }
        }

        private class MainInfo
        {
            public double Temp { get; set; }
        }

        private class WeatherDescription
        {
            public string Description { get; set; }
        }
    }

    public class WeatherInfo
    {
        public double Temperature { get; set; }
        public string Description { get; set; }
    }
}
