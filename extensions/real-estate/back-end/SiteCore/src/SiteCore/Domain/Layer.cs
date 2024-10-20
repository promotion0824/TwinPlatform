using System;

namespace SiteCore.Domain
{
    public class Layer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid LayerGroupId { get; set; }
        public string TagName { get; set; }
        public int SortOrder { get; set; }
    }
}
