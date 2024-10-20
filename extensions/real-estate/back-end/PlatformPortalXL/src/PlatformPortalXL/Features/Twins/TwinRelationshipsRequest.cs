namespace PlatformPortalXL.Features.Twins
{
    public class TwinRelationshipsRequest
    {
        public bool? ExcludeDocuments { get; set; }
        
        public bool? ExcludeAgents { get; set; }
        
        public bool? ExcludeEvents { get; set; }
        
        public bool? ExcludeCapabilities { get; set; }
    }
}
