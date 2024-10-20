namespace PlatformPortalXL.Features.Pilot
{
    public class TwinRelationshipDto
    {
        public string Id { get; set; }
        public string TargetId { get; set; }
        public string SourceId { get; set; }
        public string Name { get; set; }
        public TwinDto Target { get; set; }
        public TwinDto Source { get; set; }
        public string Substance { get; set; }
    }
}
