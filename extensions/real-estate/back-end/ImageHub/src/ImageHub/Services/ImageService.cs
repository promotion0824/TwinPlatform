using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

using Willow.Common;

using SixLabors.ImageSharp;

using ImageHub.Models;
using ImageHub.Services;

namespace Willow.ImageHub.Services
{
    public interface IImageService
    {
        Task<(Stream, RequestImageDescriptor)> GetImage(Guid rootId, string fileName, IList<string> segments);
        Task                                   DeleteImage(Guid rootId, string fileName, IList<string> segments);
        Task<ImageDescriptor>                  CreateImage(Guid rootId, IList<string> segments, IFormFile imageFile);
    }

    /// <summary>
    /// Implementation if IImageService
    /// </summary>
    public class ImageService : IImageService
    {
        private readonly IFileNameParser _fileNameParser;
        private readonly IImageRepository _sourceRepo;
        private readonly IImageRepository _cacheRepo;
        private readonly IImageEngine _processorEngine;

        public ImageService(IFileNameParser fileNameParser, IImageRepository sourceRepo, IImageRepository cacheRepo, IImageEngine processorEngine)
        {
            _fileNameParser  = fileNameParser;
            _sourceRepo      = sourceRepo;
            _cacheRepo       = cacheRepo;
            _processorEngine = processorEngine;
        }

        #region IImageService

        public async Task<(Stream, RequestImageDescriptor)> GetImage(Guid rootId, string fileName, IList<string> segments)
        {
            var requestImageDescriptor = _fileNameParser.Parse(fileName);
            var cachedId = GetCachedImageFilePath(rootId, segments, requestImageDescriptor);

            try
            {
                // First see if image has already been processed (appropriate size and format) and in cache
                var cachedImage = await _cacheRepo.Get(cachedId);

                if(cachedImage != null)
                    return (cachedImage, requestImageDescriptor);
            }
            catch(FileNotFoundException)
            {
                // Fall thru
            }

            // Not in cache
            var sourceId       = GetOriginalImageDataFilePath(rootId, segments, requestImageDescriptor.ImageId);
            var sourceImage    = await _sourceRepo.Get(sourceId);
            var processedImage = _processorEngine.Process(sourceImage, requestImageDescriptor);

            await _cacheRepo.Add(cachedId, processedImage);

            processedImage.Seek(0, SeekOrigin.Begin);

            return (processedImage, requestImageDescriptor);
        }

        public async Task DeleteImage(Guid rootId, string fileName, IList<string> segments)
        {
            var deleteSource = DeleteSourceImage(rootId, fileName, segments);
            var deleteCache  = DeleteCachedImages(rootId, fileName, segments);

            await Task.WhenAll(deleteSource, deleteCache);
        }

        public async Task<ImageDescriptor> CreateImage(Guid rootId, IList<string> segments, IFormFile imageFile)
        {
            var memoryStream = new MemoryStream();

            await imageFile.CopyToAsync(memoryStream);

            try
            { 
                memoryStream.Seek(0, SeekOrigin.Begin);

                try
                {
                    Image.Load(memoryStream);
                }
                catch(NotSupportedException ex)
                {
                    throw new ArgumentException("Invalid image file", ex);
                }

                memoryStream.Seek(0, SeekOrigin.Begin);

                var imageId = Guid.NewGuid();
                var result  = await _sourceRepo.Add(GetOriginalImageDataFilePath(rootId, segments, imageId), memoryStream);

                result.ImageId = imageId;

                return result;
            }
            finally
            {
                memoryStream.Dispose();
            }
        }

        #endregion

        public static List<string> NormalizePathSegments(params string[] pathSegments)
        {
            var result = new List<string>();

            foreach(var segment in pathSegments)
            {
                if (segment == null)
                {
                    continue;
                }

                if (Guid.TryParse(segment, out Guid g))
                {
                    result.Add(g.ToString("N").ToLowerInvariant());
                }
                else
                {
                    result.Add(segment.ToLowerInvariant());
                }
            }

            return result;
        }

        #region Path Helpers

        public static string GetOriginalImageDataFilePath(Guid rootId, IList<string> pathSegments, Guid? imageId)
        {
            var path = string.Join('/', pathSegments);

            if(imageId.HasValue)
                return $"{rootId:N}/{path}/{imageId:N}";

            return $"{rootId:N}/{path}";
        }

        public static string GetOriginalImageDescriptorFilePath(Guid rootId, IList<string> pathSegments, Guid imageId)
        {
            var path = string.Join('/', pathSegments);
            return $"{rootId:N}/{path}/{imageId:N}.json";
        }

        public static string GetCachedImageFilePath(Guid rootId, IList<string> pathSegments, RequestImageDescriptor imageDescriptor)
        {
            var path = string.Join('/', pathSegments);
            return $"{rootId:N}/{path}/{imageDescriptor.GetNormalizedFileName()}";
        }

        public static string GetCachedImageFilePathPrefix(Guid rootId, IList<string> pathSegments, Guid imageId)
        {
            var path = string.Join('/', pathSegments);
            return $"{rootId:N}/{path}/{imageId:N}";
        }

        #endregion

        #region Private

        private async Task DeleteSourceImage(Guid rootId, string fileName, IList<string> segments)
        {
            var sourceId = "";

            try 
            {
                var requestImageDescriptor = _fileNameParser.Parse(fileName);

                sourceId = GetOriginalImageDataFilePath(rootId, segments, requestImageDescriptor.ImageId);
            }
            catch
            {
                sourceId = GetOriginalImageDataFilePath(rootId, segments, Guid.Parse(fileName));
            }

            try
            {
                await _sourceRepo.Delete(sourceId);
            }
            catch(FileNotFoundException)
            {
                // Do nothing
            }

            try
            {
                // Old descriptor files
                await _sourceRepo.Delete(sourceId + ".json");
            }
            catch(FileNotFoundException)
            {
                // Do nothing
            }
        }

        private async Task DeleteCachedImages(Guid rootId, string fileName, IList<string> segments)
        {
            var parts = fileName.Split("_");
            var cachedFolder = GetCachedImageFilePathPrefix(rootId, segments, Guid.Parse(parts[0]));

            await _cacheRepo.DeleteFolder(cachedFolder);
        }

        #endregion
    }
}
