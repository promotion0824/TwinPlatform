using System;

namespace DigitalTwinCore.DTO
{
	/// <summary>
	/// Twin with a limited set of properties
	/// </summary>
	public class TwinSimpleDto
	{
		/// <summary>
		///  Twin Id 
		/// </summary>
		public string Id { get; set; }
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

		/// <summary>
		/// the FloorId of the Twin
		/// </summary>
		public Guid? FloorId { get; set; }

        /// <summary>
        /// the ModelId of the Twin
        /// </summary>
        public string ModelId { get; set; }
}
}
