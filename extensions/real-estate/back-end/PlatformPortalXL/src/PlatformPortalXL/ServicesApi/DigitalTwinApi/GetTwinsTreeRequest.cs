using System.Collections.Generic;
using System;
using System.Linq;

namespace PlatformPortalXL.ServicesApi.DigitalTwinApi
{
    public class GetTwinsTreeRequest
    {
        public GetTwinsTreeRequest()
        {
            OutgoingRelationships = [];
            IncomingRelationships = [];
        }

        public IEnumerable<string> ModelIds { get; set; }
        public IEnumerable<string> OutgoingRelationships { get; set; }
        public IEnumerable<string> IncomingRelationships { get; set; }
        public bool ExactModelMatch { get; set; }
        public IEnumerable<Guid> SiteIds { get; set; }
    }
}
