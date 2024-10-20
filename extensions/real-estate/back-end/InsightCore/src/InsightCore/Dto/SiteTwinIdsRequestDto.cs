using System;
using System.Collections.Generic;

namespace InsightCore.Dto
{
	public class SiteTwinIdsRequestDto
	{
		/// <summary>
		/// site Id of the Twins 
		/// </summary>
		public Guid SiteId { get; set; }

		/// <summary>
		/// List of the twin Ids for the site Id
		/// </summary>
		public IEnumerable<string> TwinIds { get; set; }
	}
}
