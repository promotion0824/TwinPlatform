using System.Collections.Generic;
using System.Threading.Tasks;
using PlatformPortalXL.Services;
using Willow.Api.Client;

namespace PlatformPortalXL.ServicesApi.WeatherbitApi
{
    public interface IWeatherbitApiService
    {
        Task<WeatherbitSimpleDto> GetCurrentWeatherAsync(double latitude, double longitude);
    }

    public class WeatherbitApiService : IWeatherbitApiService
    {
        private readonly IResiliencePipelineService _resiliencePipelineService;
        private readonly IRestApi _weatherbitApi;
        private readonly string _weatherbitApiUrl;
        private readonly string _weatherbitApiKey;

        public WeatherbitApiService(
            IRestApi weatherbitApi,
            string weatherbitApiUrl,
            string weatherbitApiKey,
            IResiliencePipelineService resiliencePipelineService)
        {
            _weatherbitApi = weatherbitApi;
            _weatherbitApiUrl = weatherbitApiUrl;
            _weatherbitApiKey = weatherbitApiKey;
            _resiliencePipelineService = resiliencePipelineService;
        }

        public async Task<WeatherbitSimpleDto> GetCurrentWeatherAsync(double latitude, double longitude)
        {
            var url = $"{_weatherbitApiUrl}?lat={latitude}&lon={longitude}&key={_weatherbitApiKey}";
            var response = await _resiliencePipelineService.ExecuteAsync(async _ =>
                await _weatherbitApi.Get<WeatherbitCurrentDto>(url, null));

            if (response?.Data != null && response.Data.Count > 0)
            {
                var weatherData = response.Data[0];
                return new WeatherbitSimpleDto
                {
                    Temp = weatherData.Temp,
                    Code = weatherData.Weather?.Code ?? 0,
                    Icon = weatherData.Weather?.Icon
                };
            }

            return null;
        }
    }

    public class WeatherbitSimpleDto
    {
        public decimal Temp { get; set; }
        public int Code { get; set; }
        public string Icon { get; set; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>Temp, code and Icon</returns>
        public override string ToString()
        {
            return $"Temp: ({Temp}), Code: ({Code}), Icon: ({Icon})";
        }
    }

    public class WeatherbitCurrentDto
    {
        public int Count { get; set; }
        public List<WeatherbitDataDto> Data { get; set; }
    }

    public class WeatherbitDataDto
    {
        public decimal Temp { get; set; }
        public WeatherbitWeatherDto Weather { get; set; }
    }

    public class WeatherbitWeatherDto
    {
        public int Code { get; set; }
        public string Icon { get; set; }
    }
}
