using Microsoft.IdentityModel.Tokens;

namespace PlatformPortalXL.Models.Forge
{
    public class ForgeInfo
    {
        public string BucketKey { get; set; }
        public string FileObjectId { get; set; }
        public bool IsTranslationCompleted { get; set; }

        public string Urn
        {
            get
            {
                return Base64UrlEncoder.Encode(FileObjectId);
            }
        }
    }
}
