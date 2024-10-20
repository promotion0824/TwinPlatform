using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlatformPortalXL.Features.Pilot;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Willow.Api.Binding.Binders;

namespace DigitalTwinCore.DTO
{
    public class CreateDocumentRequestBase
    {
        public CreateDocumentRequestBase()
        {
            ShareStorageForSameFile = true;
            Metadata = new TwinMetadataDto();
            CustomProperties = new Dictionary<string, object>();
        }

        public string Id { get; set; }
        public TwinMetadataDto Metadata { get; set; }
        public string FileMimeType { get; set; }
        public bool ShareStorageForSameFile { get; set; }

        public IDictionary<string, object> CustomProperties { get; set; }
    }
}
