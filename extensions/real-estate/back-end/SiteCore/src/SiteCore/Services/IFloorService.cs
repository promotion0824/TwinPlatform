using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SiteCore.Domain;
using SiteCore.Requests;

namespace SiteCore.Services
{
    public interface IFloorService
    {
		Task<List<Floor>> GetFloors(List<Guid> floorIds);
		Task<List<Floor>> GetFloors(Guid siteId, bool all);
        Task<Floor> GetFloorById(Guid siteId, Guid floorId);
        Task<Floor> GetFloorByCode(Guid siteId, string floorCode);
        Task<Floor> GetFloorBySiteId(Guid siteId);
        Task<Floor> Upload3DFloorModules(Guid siteId, Guid floorId, CreateUpdateModule3DRequest request);
        Task<Floor> Upload2DFloorModules(Guid siteId, Guid floorId, IFormFileCollection files);
        Task<Floor> UpdateFloorAsync(Guid floorId, UpdateFloorRequest updateFloorRequest);
        Task<Floor> DeleteModule(Guid floorId, Guid moduleId);
        Task InitializeSiteFloors(Guid siteId, IList<string> floorCodes);
        Task<Floor> UpdateFloorGeometryAsync(Guid floorId, UpdateFloorGeometryRequest request);
        Task UpdateSortOrder(Guid siteId, Guid[] floorIds);
        Task<bool> IsFloorExistByCode(Guid siteId, string floorCode);
        Task<Floor> CreateFloor(Guid siteId, CreateFloorRequest createFloorRequest);
        Task DeleteFloor(Guid siteId, Guid floorId);
    }
}
