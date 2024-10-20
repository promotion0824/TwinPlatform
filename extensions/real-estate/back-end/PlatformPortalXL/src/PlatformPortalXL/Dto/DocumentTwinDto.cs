using System;

namespace PlatformPortalXL.Dto
{
    public class DocumentTwinDto
    {
        public string DisplayName { get; set; }
        public Guid Id { get; set; }
        public string ModelId { get; set; }
        public Uri Url { get; set; }
        public Guid UniqueId { get; set; }
    }
}
