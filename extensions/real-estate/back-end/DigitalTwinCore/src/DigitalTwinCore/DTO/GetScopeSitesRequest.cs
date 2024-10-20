using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DigitalTwinCore.DTO
{
    public class GetScopeSitesRequest
    {
        public ScopeRequest Scope { get; set; }
    }
    public class ScopeRequest
    {
        [Required]
        public string DtId { get; set; }

        public List<Guid> SiteIds { get; set; }
    }
}
