using System;
using Willow.Batch;

namespace PlatformPortalXL.Features.SiteStructure.Requests
{
    public class BatchSitesRequest : BatchRequestDto
    {
        public ProjectField[] ProjectFields { get; set; } = Array.Empty<ProjectField>();
    }

    public class ProjectField
    {
        public string Name { get; set; }
    }
}
