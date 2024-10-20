using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.GeometryViewer
{
    public interface IGeometryViewerService
    {
        Task<List<string>> GetGeometryViewerIds(string urn);
    }
}
