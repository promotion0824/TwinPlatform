using System;
using Microsoft.Extensions.Configuration;

namespace PlatformPortalXL.Services
{
    public interface IImageUrlHelper
    {
        string GetCustomerLogoUrl(string path, Guid logoId);
        string GetSiteLogoUrl(string path, Guid logoId);
        string GetSiteLogoOriginalSizeUrl(string path, Guid logoId);
        string GetModuleUrl(string path, Guid visualId);

        string GetAppIconUrl(string path, Guid iconId);
        string GetAppGalleryImagePath(string path, Guid imageId);

        string GetAttachmentUrl(string path, Guid attachmentId);
        string GetAttachmentPreviewUrl(string path, Guid attachmentId);
        string GetSiteLogoPath(Guid customerId, Guid siteId);
    }

    public class ImageUrlHelper : IImageUrlHelper
    {
        private readonly string _imageHubPublicRootUrl;
        private readonly string _marketPlaceImageHubPublicRootUrl;

        public ImageUrlHelper(IConfiguration configuration)
        {
            _imageHubPublicRootUrl = configuration.GetValue<string>("ImageHubPublicRootUrl");
            _marketPlaceImageHubPublicRootUrl = configuration.GetValue<string>("MarketPlaceImageHubPublicRootUrl");
        }

        public string GetCustomerLogoUrl(string path, Guid logoId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{logoId}_0.png";
        }

        public string GetSiteLogoUrl(string path, Guid logoId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{logoId}_1_w300_h420.jpg";
        }

        public string GetSiteLogoOriginalSizeUrl(string path, Guid logoId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{logoId}_0.jpg";
        }

        public string GetModuleUrl(string path, Guid visualId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{visualId}_original.png";
        }

        public string GetAppGalleryImagePath(string path, Guid imageId)
        {
            return $"{_marketPlaceImageHubPublicRootUrl}/{path}/{imageId}_1_w800_h400.jpg";
        }

        public string GetAppIconUrl(string path, Guid iconId)
        {
            return $"{_marketPlaceImageHubPublicRootUrl}/{path}/{iconId}_1_w126_h126.png";
        }

        public string GetAttachmentUrl(string path, Guid attachmentId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{attachmentId}_0.jpg";
        }

        public string GetAttachmentPreviewUrl(string path, Guid attachmentId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{attachmentId}_1_w100_h100.jpg";
        }

        public string GetSiteLogoPath(Guid customerId, Guid siteId)
        {
            return $"{customerId}/sites/{siteId}/logo";
        }

    }
}
