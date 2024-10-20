using System.Collections.Generic;
using System;

namespace DigitalTwinCore.DTO
{
	/// <summary>
	/// Request for Multi sites Assets Names
	/// </summary>
	public class TwinsForMultiSitesRequest
	{
		public TwinsForMultiSitesRequest()
		{
			TwinIds = new List<string>();
		}

		/// <summary>
		/// site Id of the Assets 
		/// </summary>
		public Guid SiteId { get; set; }

		/// <summary>
		/// List of the assets Ids grouped by the site Id
		/// </summary>
		public List<string> TwinIds { get; set; }
	}
}
