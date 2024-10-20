using System;

namespace PlatformPortalXL.Models
{
    public class PointTimeSeriesRawData : TimeSeriesData
    {
        public string Id { get; set; }

        [Obsolete]
        public Guid PointEntityId { get; set; }

        public double Value { get; set; }
    }
}
