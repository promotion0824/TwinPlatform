namespace Willow.LiveData.Core.Features.Connectors.Models;

using System;
using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
internal class ConnectorStats
{
    public Guid ConnectorId { get; set; }

    public int CapabilitiesCount { get; set; }

    public int DisabledCapabilitiesCount { get; set; }

    public int EnabledCapabilitiesCount => CapabilitiesCount - DisabledCapabilitiesCount;

    public int HostingDevicesCount { get; set; }
}
