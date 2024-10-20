using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Willow.Api.Binding.Binders;

namespace DigitalTwinCore.DTO
{
    [ModelBinder(typeof(DtoWithFormFileCollectionModelBinder), Name = "data")]
    public class CreateDocumentRequest : CreateDocumentRequestBase
    {
        public IFormFileCollection formFiles { get; set; }
    }
}
