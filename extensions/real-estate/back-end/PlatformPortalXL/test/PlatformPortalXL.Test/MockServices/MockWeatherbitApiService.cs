using PlatformPortalXL.Dto;
using PlatformPortalXL.ServicesApi.WeatherbitApi;
using System.Threading.Tasks;

namespace PlatformPortalXL.Test.MockServices
{
    public class MockWeatherbitApiService : IWeatherbitApiService
    {
        public Task<WeatherbitSimpleDto> GetCurrentWeatherAsync(double latitude, double longitude)
        {
            return Task.FromResult(new WeatherbitSimpleDto
            {
                Temp = 14.4m,
                Code = 802,
                Icon = "c02n"
            });
        }

        public static WeatherDto ExpectedDto =>
            new WeatherDto
            {
                Temperature = 14.4m,
                Code = 802,
                Icon = "c02n"
            };
    }
}
