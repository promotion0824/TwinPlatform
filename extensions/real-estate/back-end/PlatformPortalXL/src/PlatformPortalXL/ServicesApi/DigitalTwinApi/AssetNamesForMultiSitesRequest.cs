using System.Collections.Generic;
using System;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
	/// <summary>
	/// Request for Multi sites Assets Names
	/// </summary>
	public class AssetNamesForMultiSitesRequest
	{
		public AssetNamesForMultiSitesRequest()
		{
			AssetsIds = new List<Guid>();
		}

		/// <summary>
		/// site Id of the Assets 
		/// </summary>
		public Guid SiteId { get; set; }

		/// <summary>
		/// List of the assets Ids grouped by the site Id
		/// </summary>
		public List<Guid> AssetsIds { get; set; }
	}
}
