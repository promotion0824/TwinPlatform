using Microsoft.AspNetCore.Http;
using PlatformPortalXL.Models.Forge;
using System;
using System.IO;

namespace PlatformPortalXL.Services.Forge
{
    public class ForgeOperationContext
    {
        public IForgeApi ForgeApi { get; }
        public Guid Id { get; }
        public Guid SiteId { get; }
        public IFormFile File { get; }
        public ForgeInfo ForgeInfo { get; }
        public string Token { get; }
        public string BucketPostfix { get; }

        public ForgeOperationContext(IForgeApi forgeApi, Guid siteId, string token, IFormFile file, string bucketPostfix)
        {
            ForgeApi = forgeApi;
            Id = Guid.NewGuid();
            SiteId = siteId;
            File = file;
            ForgeInfo = new ForgeInfo();
            Token = token;
            BucketPostfix = bucketPostfix;
        }

        public byte[] GetFileBytes()
        {
            using (var stream = new MemoryStream())
            {
                File.OpenReadStream().CopyTo(stream);
                return stream.ToArray();
            }
        }

    }
}
