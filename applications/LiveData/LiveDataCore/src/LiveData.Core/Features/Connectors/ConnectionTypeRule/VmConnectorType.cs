namespace Willow.LiveData.Core.Features.Connectors.ConnectionTypeRule;

using Microsoft.Extensions.Options;
using Willow.LiveData.Core.Infrastructure.Configuration;

internal class VmConnectorType(IOptions<TelemetryConfiguration> telemetryOptions) : BaseConnectorType(telemetryOptions), IVmConnectorType
{
}
