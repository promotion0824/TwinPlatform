using System;

namespace PlatformPortalXL.Features.SiteStructure.Requests
{
    public class NewAdtSiteRequest
    {
        /// <summary>
        /// Site id
        /// </summary>
        public Guid SiteId { get; set; }

        /// <summary>
        /// Digital Twins instance Uri
        /// </summary>
        public Uri InstanceUri { get; set; }

        /// <summary>
        /// Site code for model id
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// ADX database name
        /// </summary>
        public string AdxDatabase { get; set; }
    }
}
