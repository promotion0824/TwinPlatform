using DigitalTwinCore.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Willow.Api.Binding.Binders;

namespace DigitalTwinCore.DTO
{
    [ModelBinder(typeof(DtoWithFormFileCollectionModelBinder), Name = "data")]
    public class CreateDocumentRequest
    {
        public CreateDocumentRequest()
        {
            ShareStorageForSameFile = true;
            Metadata = new TwinMetadataDto();
            CustomProperties = new Dictionary<string, object>();
        }

        public string Id { get; set; }
        public TwinMetadataDto Metadata { get; set; }
        public string FileMimeType { get; set; }
        public bool ShareStorageForSameFile { get; set; }
        [JsonExtensionData]
        public IDictionary<string, object> CustomProperties { get; set; }
        public IFormFileCollection formFiles { get; set; }
    }
}
