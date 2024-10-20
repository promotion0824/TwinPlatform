using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using PlatformPortalXL.Models;
using Willow.Common;

namespace PlatformPortalXL.Services
{
    public interface IFileService
    {
	    Task<FileStreamResult> GetFileAsync(string filename);
    }

    public class FileService : IFileService
    {
        private readonly IBlobStore _blobStore;
        private readonly IContentTypeProvider _fileExtensionContentTypeProvider;

        public FileService(IBlobStore blobStore,
				IContentTypeProvider fileExtensionContentTypeProvider)
        {
            _blobStore = blobStore;
            _fileExtensionContentTypeProvider = fileExtensionContentTypeProvider ??
                                                throw new ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        /// <summary>
        /// Load a file from blob storage.
        /// </summary>
        public async Task<FileStreamResult> GetFileAsync(string filename)
        {
	        try
	        {
		        var content = new MemoryStream();

		        await _blobStore.Get(filename, content);

		        if (!_fileExtensionContentTypeProvider.TryGetContentType(filename, out var contentType))
		        {
			        contentType = "application/octet-stream";
		        }

		        content.Position = 0;

		        return new FileStreamResult
		        {
			        Content = content,
			        ContentType = new MediaTypeHeaderValue(contentType),
			        FileName = filename
		        };
	        }
	        catch (Exception ex)
	        {
		        ex.Data.Add("Filename", filename);
		        throw;
	        }
        }
    }
}
