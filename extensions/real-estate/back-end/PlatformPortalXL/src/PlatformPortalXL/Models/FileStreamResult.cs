using System.IO;
using System.Net.Http.Headers;

namespace PlatformPortalXL.Models
{
    public class FileStreamResult
    {
        public Stream Content { get; set; }

        public MediaTypeHeaderValue ContentType { get; set; }

        public string FileName { get; set; }
    }
}
