using System.Text.Json;
using Willow.Model.Adt;

namespace Willow.Model.Adx.Model;

public class DigitalTwinModelExportData
{
    public DigitalTwinModelExportData(DigitalTwinsModelBasicData model)
    {
        Id = model.Id;
        DtdlModel = JsonDocument.Parse(model.DtdlModel);
        CustomProperties = new();
    }

    public JsonDocument DtdlModel { get; set; }

    public Dictionary<string, string> CustomProperties { get; set; }

    /// <summary>
    /// DTDL model's id
    /// </summary>
    public string Id { get; set; }
}
