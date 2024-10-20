using System;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{

	/// <summary>
	/// Twin with a limited set of properties
	/// </summary>
	public class TwinSimpleResponse
	{
		/// <summary>
		///  Twin Id 
		/// </summary>
		public string TwinId { get; set; }
		/// <summary>
		///  the Site Id of the Twin
		/// </summary>
		public Guid SiteId { get; set; }

		/// <summary>
		///  the unique Id of the Twin 
		/// </summary>
		public Guid UniqueId { get; set; }

		/// <summary>
		///  the name of the Twin 
		/// </summary>
		public string Name { get; set; }
	}
}
