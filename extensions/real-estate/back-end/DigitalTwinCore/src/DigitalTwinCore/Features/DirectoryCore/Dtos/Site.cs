using System;

namespace DigitalTwinCore.Features.DirectoryCore.Dtos;

public class Site
{
    public Guid Id { get; set; }

    public Guid CustomerId { get; set; }

    public string Name { get; set; }

    public string Code { get; set; }

    public SiteStatus Status { get; set; }

    public SiteFeatures Features { get; set; }

    public TimeZoneInfo Timezone { get; set; }

    public string WebMapId { get; set; }
}