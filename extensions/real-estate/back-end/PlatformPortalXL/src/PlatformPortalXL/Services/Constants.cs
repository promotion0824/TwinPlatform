namespace PlatformPortalXL.Services
{
    public static class Constants
    {
        /// <summary>
        /// The delay between each call to the weather API.
        /// </summary>
        public const int InterCallDelayMilliseconds = 1000;

        /// <summary>
        /// How often the cache is refreshed.
        /// </summary>
        public const int WeatherUpdateIntervalMinutes = 60;

        /// <summary>
        /// The duration from now after which the cache entry will expire.
        /// </summary>
        public const int CacheExpiryHours = 2;
    }
}
