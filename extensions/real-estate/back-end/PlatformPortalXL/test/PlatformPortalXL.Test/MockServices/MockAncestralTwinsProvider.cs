using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlatformPortalXL.Auth;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Test.MockServices;

public class MockAncestralTwinsSearchService: IAncestralTwinsSearchService
{
    public Task<IEnumerable<ITwinWithAncestors>> GetTwinsByModel(string model, int page, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Enumerable.Empty<ITwinWithAncestors>());
    }

    public Task<ITwinWithAncestors> GetTwinById(string twinId, CancellationToken cancellationToken = default)
    {
        ITwinWithAncestors result = new TwinWithAncestors(twinId, []);
        return Task.FromResult(result);
    }
}
