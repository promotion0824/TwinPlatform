namespace PlatformPortalXL.Features.SiteStructure.Requests
{
    public class ModuleTypeRequest
    {
        public string Name { get; set; }

        public string Prefix { get; set; }

        public string ModuleGroup { get; set; }

        public bool Is3D { get; set; }

        public int SortOrder { get; set; }

        public bool CanBeDeleted { get; set; }

        public bool IsDefault { get; set; }
    }
}
