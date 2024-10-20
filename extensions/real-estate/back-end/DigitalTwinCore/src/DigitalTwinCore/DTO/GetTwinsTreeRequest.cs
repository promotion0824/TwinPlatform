using System.Collections.Generic;
using System;
using System.Linq;

namespace DigitalTwinCore.DTO
{
    public class GetTwinsTreeRequest
    {
        public GetTwinsTreeRequest()
        {
            OutgoingRelationships = Enumerable.Empty<string>().ToList();
            IncomingRelationships = Enumerable.Empty<string>().ToList();
        }
        public List<string> ModelIds { get; set; }
        public List<string> OutgoingRelationships { get; set; }
        public List<string> IncomingRelationships { get; set; }
        public bool ExactModelMatch { get; set; }
        public List<Guid> SiteIds { get; set; }
    }
}
