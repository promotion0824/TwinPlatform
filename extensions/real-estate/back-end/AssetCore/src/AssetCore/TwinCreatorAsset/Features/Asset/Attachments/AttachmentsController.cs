using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Dto;
using AssetCoreTwinCreator.Helper;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.MappingId.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Infrastructure.Exceptions;

using Willow.Common;

namespace AssetCoreTwinCreator.Features.Asset.Attachments
{
    public class AttachmentsController : Controller
    {
        private readonly IAttachmentsService _attachmentsService;
        private readonly IBlobStore _blobStore;
        private readonly IMappingService _mappingService;

        public AttachmentsController(IAttachmentsService attachmentsService, IBlobStore blobStore, IMappingService mappingService)
        {
            _attachmentsService = attachmentsService;
            _blobStore = blobStore;
            _mappingService = mappingService;
        }

        /// <summary>
        /// Retrieves asset files by category
        /// </summary>
        [HttpGet("api/sites/{siteId}/categories/{categoryId}/files")]
        [Authorize]
        [SwaggerOperation(OperationId = "getFiles", Tags = new string[] { "TwinCreator" })]
        public async Task<ActionResult<IEnumerable<FileDto>>> GetFiles([FromRoute]Guid categoryId, List<Guid> assetIds)
        {
            try
            {
                var assetIdsInt = assetIds.Select(id => id.ToAssetId()).ToList();
                var categoryIdInt = categoryId.ToCategoryId();
                var files = await _attachmentsService.GetFiles(categoryIdInt, assetIdsInt);
                var dtos = await _mappingService.MapAssetFiles(files);
                return dtos;
            }
            catch(ArgumentException)
            {
                return BadRequest();
            }
        }

        [HttpGet("api/sites/{siteId}/files/{fileId}/content")]
        [Authorize]
        [ProducesResponseType(typeof(System.IO.MemoryStream), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [SwaggerOperation(OperationId = "getFileContent", Tags = new string[] { "TwinCreator" })]
        public async Task<IActionResult> GetFileContent([FromRoute] Guid siteId, [FromRoute] Guid fileId)
        {
            var fileIdInt = fileId.ToFileId();

            var file = await _attachmentsService.GetFile(fileIdInt);
            if (file == null)
            {
                throw new ResourceNotFoundException(nameof(Models.File), fileId);
            }

            var content = new MemoryStream();
            
            await _blobStore.Get(file.BlobName, content);

            var mimeType = MimeTypeHelper.GetMimeType(file.FileName);

            content.Position = 0;
            var result = File(content, mimeType, file.FileName);

            return result;
        }
    }
}