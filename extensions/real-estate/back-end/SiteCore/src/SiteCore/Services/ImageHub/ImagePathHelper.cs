using System;

namespace SiteCore.Services.ImageHub
{
    public interface IImagePathHelper
    {
        string GetSiteLogoPath(Guid customerId, Guid siteId);
        string GetFloorModulePath(Guid customerId, Guid siteId, Guid floorId);
        string GetFloorModulePath(Guid customerId, Guid siteId, Guid floorId, Guid imageId);
    }

    public class ImagePathHelper : IImagePathHelper
    {
        public string GetSiteLogoPath(Guid customerId, Guid siteId)
        {
            return $"{customerId}/sites/{siteId}/logo";
        }

        public string GetFloorModulePath(Guid customerId, Guid siteId, Guid floorId)
        {
            return $"{customerId}/sites/{siteId}/floors/{floorId}/modules";
        }

        public string GetFloorModulePath(Guid customerId, Guid siteId, Guid floorId, Guid imageId)
        {
            return $"{customerId}/sites/{siteId}/floors/{floorId}/modules/{imageId}";
        }
    }
}
