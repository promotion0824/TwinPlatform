using System;

namespace PlatformPortalXL.Services.ArcGis
{
    public class ArcGisOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string GisBaseUrl { get; set; }
        public string GisPortalPath { get; set; }
        public string GisTokenPath { get; set; }
        public string AuthRequiredPaths { get; set; }
        public string DefaultWebMapId { get; set; }
		public Guid CustomerId { get; set; }
	}
}
