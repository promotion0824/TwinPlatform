using System.ComponentModel.DataAnnotations;

namespace PlatformPortalXL.Features.Scopes
{
    public class GetScopeSitesRequest
    {
        public ScopeRequest Scope { get; set; }
    }
    public class ScopeRequest
    {
        [Required]
        public string DtId { get; set; }
    }
}
