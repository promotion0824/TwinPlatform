using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ImageHub.Models;
using ImageHub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;

using Willow.Common;
using Willow.ImageHub.Services;

namespace ImageHub.Controllers
{
    [ApiController]
    #if !DEBUG
    [Authorize]
    #endif
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class ImagesController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService)
        {
            _imageService = imageService;
        }

        #region GetImage

        [HttpGet("{rootId}/{pathSegment0}/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetImageWith1Segment(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string fileName)
        {
            return await GetImage(rootId, fileName, pathSegment0);
        }

        [HttpGet("{rootId}/{pathSegment0}/{pathSegment1}/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetImageWith2Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string fileName)
        {
            return await GetImage(rootId, fileName, pathSegment0, pathSegment1);
        }

        [HttpGet("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetImageWith3Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string fileName)
        {
            return await GetImage(rootId, fileName, pathSegment0, pathSegment1, pathSegment2);
        }

        [HttpGet("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetImageWith4Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            [FromRoute] string fileName)
        {
            return await GetImage(rootId, fileName, pathSegment0, pathSegment1, pathSegment2, pathSegment3);
        }

        [HttpGet("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}/{pathSegment4}/{fileName}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetImageWith5Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            [FromRoute] string pathSegment4,
            [FromRoute] string fileName)
        {
            return await GetImage(rootId, fileName, pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);
        }

        #endregion

        #region CreateImage

        [HttpPost("{rootId}/{pathSegment0}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageDescriptor), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateImageWith1Segment(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            IFormFile imageFile)
        {
            return await CreateImage(rootId, imageFile, pathSegment0);
        }

        [HttpPost("{rootId}/{pathSegment0}/{pathSegment1}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageDescriptor), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateImageWith2Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            IFormFile imageFile)
        {
            return await CreateImage(rootId, imageFile, pathSegment0, pathSegment1);
        }

        [HttpPost("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageDescriptor), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateImageWith3Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            IFormFile imageFile)
        {
            return await CreateImage(rootId, imageFile, pathSegment0, pathSegment1, pathSegment2);
        }

        [HttpPost("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageDescriptor), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateImageWith4Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            IFormFile imageFile)
        {
            return await CreateImage(rootId, imageFile, pathSegment0, pathSegment1, pathSegment2, pathSegment3);
        }

        [HttpPost("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}/{pathSegment4}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ImageDescriptor), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CreateImageWith5Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            [FromRoute] string pathSegment4,
            IFormFile imageFile)
        {
            return await CreateImage(rootId, imageFile, pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);
        }
        
        #endregion

        #region DeleteImage

        [HttpDelete("{rootId}/{pathSegment0}/{imageId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteImageWith1Segment(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] Guid imageId)
        {
            return await DeleteImage(rootId, imageId, pathSegment0);
        }

        [HttpDelete("{rootId}/{pathSegment0}/{pathSegment1}/{imageId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteImageWith2Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] Guid imageId)
        {
            return await DeleteImage(rootId, imageId, pathSegment0, pathSegment1);
        }

        [HttpDelete("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{imageId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteImageWith3Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] Guid imageId)
        {
            return await DeleteImage(rootId, imageId, pathSegment0, pathSegment1, pathSegment2);
        }

        [HttpDelete("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}/{imageId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteImageWith4Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            [FromRoute] Guid imageId)
        {
            return await DeleteImage(rootId, imageId, pathSegment0, pathSegment1, pathSegment2, pathSegment3);
        }

        [HttpDelete("{rootId}/{pathSegment0}/{pathSegment1}/{pathSegment2}/{pathSegment3}/{pathSegment4}/{imageId}")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteImageWith5Segments(
            [FromRoute] Guid rootId,
            [FromRoute] string pathSegment0,
            [FromRoute] string pathSegment1,
            [FromRoute] string pathSegment2,
            [FromRoute] string pathSegment3,
            [FromRoute] string pathSegment4,
            [FromRoute] Guid imageId)
        {
            return await DeleteImage(rootId, imageId, pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);
        }

        #endregion

        #region Private

        private async Task<IActionResult> GetImage(
            Guid rootId,
            string fileName,
            string pathSegment0,
            string pathSegment1 = null,
            string pathSegment2 = null,
            string pathSegment3 = null,
            string pathSegment4 = null)
        {
            var segments = NormalizePathSegments(pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);

            (Stream Content, RequestImageDescriptor Descriptor) result = await _imageService.GetImage(rootId, fileName, segments);

            return File(result.Content, GetContentType(result.Descriptor.FileExtension), result.Descriptor.GetNormalizedFileName());
        }

        private async Task<IActionResult> CreateImage(
            Guid rootId,
            IFormFile imageFile,
            string pathSegment0,
            string pathSegment1 = null,
            string pathSegment2 = null,
            string pathSegment3 = null,
            string pathSegment4 = null)
        {
            var segments = NormalizePathSegments(pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);

            var descriptor = await _imageService.CreateImage(rootId,  segments, imageFile);

            return Ok(descriptor);
        }

        private async Task<IActionResult> DeleteImage(
            Guid rootId,
            Guid imageId,
            string pathSegment0,
            string pathSegment1 = null,
            string pathSegment2 = null,
            string pathSegment3 = null,
            string pathSegment4 = null)
        {
            var segments = NormalizePathSegments(pathSegment0, pathSegment1, pathSegment2, pathSegment3, pathSegment4);
            
            await _imageService.DeleteImage(rootId, imageId.ToString(), segments);

            return NoContent();
        }

        private string GetContentType(ImageFileExtension fileExtension)
        {
            switch(fileExtension)
            {
                case ImageFileExtension.Jpeg:
                    return "image/jpeg";
                case ImageFileExtension.Png:
                    return "image/png";
                default:
                    throw new ArgumentException($"Unknown image file extension {fileExtension}", nameof(fileExtension));
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
    }
}
