using System;
using System.Threading.Tasks;
using Willow.Api.Client;

namespace PlatformPortalXL.ServicesApi.GeometryViewerApi
{
    /// <summary>
    /// Define methods to work with GeometryViewer
    /// </summary>
    public interface IGeometryViewerApiService
    {
        Task AddGeometryViewerModel(GeometryViewerModel request);
        Task<bool> ExistsGeometryViewerModel(string urn);
    }

    public class GeometryViewerApiService : IGeometryViewerApiService
    {
        private readonly IRestApi _digitalTwinCoreApi;

        public GeometryViewerApiService(IRestApi digitalTwinCoreApi)
        {
            _digitalTwinCoreApi = digitalTwinCoreApi ?? throw new ArgumentNullException(nameof(digitalTwinCoreApi));
        }

        public async Task AddGeometryViewerModel(GeometryViewerModel request)
        {
			await _digitalTwinCoreApi.PostCommand("admin/geometryviewer", request);
        }

        public async Task<bool> ExistsGeometryViewerModel(string urn)
        {
            try
            {
				await _digitalTwinCoreApi.Get<bool?>($"admin/geometryviewer/{urn}");

                return true;
            }
            catch (RestException ex)
            {
				if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					return false;
				}

				throw;
            }
        }
    }
}

