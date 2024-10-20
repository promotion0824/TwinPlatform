using DigitalTwinCore.Constants;
using DigitalTwinCore.Controllers;
using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Willow.Common;

namespace DigitalTwinCore.Services
{
    public interface IDocumentsService
    {
        Task<TwinWithRelationships> CreateFileTwin(Guid siteId, CreateDocumentRequest createDocumentRequest, IFormFile formFile, string blobName, bool syncRequired = true);
        Task<string> UploadFile(string fileMimeType, IFormFile formFile, bool shareStorageForSameFile);
        Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid siteId, Guid twinUniqueId);
        Task<RelationshipDto> AddRelationshipAsync(Guid siteId, string twinId, string documentId);
        Task DeleteRelationshipAsync(Guid siteId, string twinId, string documentId);
    }

    public class DocumentsService : IDocumentsService
    {
        private readonly IContentTypeProvider _contentTypeProvider;
        private readonly IHashCreator _hashCreator;
        private readonly IBlobStore _blobStore;
        private readonly IDigitalTwinServiceProvider _digitalTwinServiceFactory;
        private readonly IGuidWrapper _guidWrapper;
        private readonly string _blobUri;

        public DocumentsService(IContentTypeProvider contentTypeProvider, 
            IHashCreator hashCreator, 
            IBlobStore blobStore,
            IDigitalTwinServiceProvider digitalTwinServiceFactory, 
            IGuidWrapper guidWrapper,
            string blobUri)
        {
            _contentTypeProvider = contentTypeProvider;
            _hashCreator = hashCreator;
            _blobStore = blobStore;
            _digitalTwinServiceFactory = digitalTwinServiceFactory;
            _guidWrapper = guidWrapper;
            _blobUri = blobUri;
        }

        public async Task<TwinWithRelationships> CreateFileTwin(Guid siteId, CreateDocumentRequest createDocumentRequest, IFormFile formFile, string blobName, bool syncRequired = true)
        {            
            var service = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var twin = new Twin
            {
                Id = string.IsNullOrWhiteSpace(createDocumentRequest.Id) ? _guidWrapper.NewGuid().ToString() : createDocumentRequest.Id,
                Metadata = new TwinMetadata()
                {
                    ModelId = string.IsNullOrWhiteSpace(createDocumentRequest.Metadata.ModelId) ? WillowInc.DocumentModelId : createDocumentRequest.Metadata.ModelId,
                },
                CustomProperties = createDocumentRequest.CustomProperties ?? new Dictionary<string, object>()
            };

            twin.CustomProperties["url"] = new Uri(new Uri(_blobUri), blobName);
            if (!twin.CustomProperties.ContainsKey("name"))
                twin.CustomProperties.Add("name", formFile.FileName);

            var entity = await service.AddOrUpdateTwinAsync(twin, syncRequired);
            return entity;
        }

        public async Task<string> UploadFile(string fileMimeType, IFormFile formFile, bool shareStorageForSameFile)
        {
            using var fileStream = formFile.OpenReadStream();
            var md5Hash = _hashCreator.Create(fileStream);

            if (shareStorageForSameFile)
                return (await _blobStore.PutIfTagsNotExist(formFile.FileName, fileStream, new { Md5Hash = Convert.ToBase64String(md5Hash) }, new string[] { "Md5Hash" }  )).First();

            await _blobStore.Put(formFile.FileName, fileStream, new { Md5Hash = Convert.ToBase64String(md5Hash) });

            return formFile.FileName;
        }

        public async Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid siteId, Guid twinUniqueId)
        {
            var service = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var twin = await service.GetTwinByUniqueIdAsync(twinUniqueId);

            return twin;
        }

        public async Task<RelationshipDto> AddRelationshipAsync(Guid siteId, string twinId, string documentId)
        {
            var service = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            var relationship = await service.GetRelationshipAsync(twinId, documentId);

            relationship ??= await service.AddRelationshipAsync(twinId, documentId, new Relationship
            {
                SourceId = twinId,
                TargetId = documentId,
                Name = Relationships.HasDocument
            });

            return RelationshipDto.MapFrom(relationship);
        }

        public async Task DeleteRelationshipAsync(Guid siteId, string twinId, string documentId)
        {
            var service = await _digitalTwinServiceFactory.GetForSiteAsync(siteId);

            await service.DeleteRelationshipAsync(twinId, documentId);
        }
    }
}
