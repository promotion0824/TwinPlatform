using System.IO;

namespace PlatformPortalXL.ServicesApi.AssetApi
{
    public class FileDownload
    {
        public Stream Stream { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
    }
}
