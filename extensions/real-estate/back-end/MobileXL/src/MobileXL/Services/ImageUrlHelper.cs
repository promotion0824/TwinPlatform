using System;
using Microsoft.Extensions.Configuration;

namespace MobileXL.Services
{
    public interface IImageUrlHelper
    {
        string GetAttachmentUrl(string path, Guid attachmentId);
        string GetAttachmentPreviewUrl(string path, Guid attachmentId);
    }

    public class ImageUrlHelper : IImageUrlHelper
    {
        private readonly string _imageHubPublicRootUrl;

        public ImageUrlHelper(IConfiguration configuration)
        {
            _imageHubPublicRootUrl = configuration.GetValue<string>("ImageHubPublicRootUrl");
        }

        public string GetAttachmentUrl(string path, Guid attachmentId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{attachmentId}_0.jpg";
        }

        public string GetAttachmentPreviewUrl(string path, Guid attachmentId)
        {
            return $"{_imageHubPublicRootUrl}/{path}/{attachmentId}_1_w80_h80.jpg";
        }

    }
}
