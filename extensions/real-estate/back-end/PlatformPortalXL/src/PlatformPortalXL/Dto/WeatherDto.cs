
namespace PlatformPortalXL.Dto
{
    public class WeatherDto
    {
        /// <summary>
        /// Gets or sets the temperature in Celsius.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Gets or sets the weather code. See https://www.weatherbit.io/api/codes for reference.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the weather icon code.
        /// </summary>
        public string Icon { get; set; }
    }
}
