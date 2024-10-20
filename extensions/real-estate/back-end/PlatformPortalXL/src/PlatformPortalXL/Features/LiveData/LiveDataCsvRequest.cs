using System;

namespace PlatformPortalXL.Features.LiveData
{
	public class LiveDataCsvRequest
	{
		public SitePoint[] Points { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public string Interval { get; set; }
        public string TimeZoneId { get; set; }
    }

	public class SitePoint
	{
		public Guid PointId { get; set; }
		public Guid SiteId { get; set; }
	}
}
