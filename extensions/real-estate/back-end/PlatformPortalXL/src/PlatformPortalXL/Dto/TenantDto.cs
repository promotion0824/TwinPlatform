using PlatformPortalXL.Models;
using System;

namespace PlatformPortalXL.Dto
{
	/// <summary>
	/// Tenant data
	/// </summary>
	public class TenantDto
	{
		/// <summary>
		///  Site unique id for the tenant
		/// </summary>
		public Guid SiteId { get; set; }
		/// <summary>
		///  Tenant unit twin id for the tenant
		/// </summary>
		public string TenantUnitId { get; set; }
		/// <summary>
		///  Lease twin id for the tenant
		/// </summary>
		public string LeaseId { get; set; }
		/// <summary>
		///  Tenant twin id
		/// </summary>
		public string TenantId { get; set; }
		/// <summary>
		///  Tenant name
		/// </summary>
		public string TenantName { get; set; }
		/// <summary>
		///  Tenant unique id
		/// </summary>
		public Guid TenantUniqueId { get; set; }
	}
}
