using System;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class AssetPinDto
    {
        public class LiveDataPoint
        {
            public Guid Id { get; set; }
            public string Tag { get; set; }
            public string Unit { get; set; }
            public DateTime? LiveDataTimestamp { get; set; }
            public double? LiveDataValue { get; set; }
            public decimal? DisplayPriority { get; set; }
        }

        public string Title { get; set; }
        public List<LiveDataPoint> LiveDataPoints { get; set; }
    }
}
