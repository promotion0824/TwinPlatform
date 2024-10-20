namespace Willow.Infrastructure.MultiRegion
{
    using System;

    internal static class MultiRegionHelper
    {
        public static string ServiceName(string serviceBaseName, string regionId)
        {
            return $"{serviceBaseName}--{regionId}";
        }

        public static bool IsMultRegionUrl(string url)
        {
            return url.Contains("{regionId}", StringComparison.InvariantCultureIgnoreCase);
        }

        public static string MultiRegionUrl(string url, string regionId)
        {
            return url.Replace("{regionId}", regionId, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
