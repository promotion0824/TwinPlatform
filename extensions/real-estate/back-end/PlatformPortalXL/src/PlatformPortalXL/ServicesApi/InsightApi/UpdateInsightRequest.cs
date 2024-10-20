using System;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.ServicesApi.InsightApi
{
	public class UpdateInsightRequest
    {


	    [Obsolete("Use the LastStatus, this enum is not in use")]
	    public OldInsightStatus? Status { get; set; }
	    public InsightStatus? LastStatus { get; set; }
	    public Guid? UpdatedByUserId { get; set; }
		public string Reason { get; set; }
        public bool Reported { get; set; }
    }
}
