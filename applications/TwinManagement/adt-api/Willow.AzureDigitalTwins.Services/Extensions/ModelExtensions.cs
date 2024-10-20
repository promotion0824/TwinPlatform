using DTDLParser.Models;
using Willow.Model.Adt;

namespace Willow.AzureDigitalTwins.Services.Extensions;

public static class ModelExtensions
{
    public static DigitalTwinsModelBasicData ToModelBasicData(this (DTInterfaceInfo, DateTimeOffset?) interfaceInfo)
    {
        return new DigitalTwinsModelBasicData
        {
            Id = interfaceInfo.Item1.Id.AbsoluteUri,
            DtdlModel = interfaceInfo.Item1.GetJsonLdText(),
            UploadedOn = interfaceInfo.Item2
        };
    }
}
