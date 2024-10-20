using System.IO;
using System.Net.Http.Headers;

namespace MobileXL.Models
{
    public class FileStreamResult
    {
        public Stream Content { get; set; }

        public MediaTypeHeaderValue ContentType { get; set; }

        public string FileName { get; set; }
    }
}
