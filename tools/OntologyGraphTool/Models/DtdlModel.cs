using Newtonsoft.Json;

namespace OntologyGraphTool.Models;

public class DtdlModel : IEquatable<DtdlModel>
{
    [JsonProperty("@id")]
    public string id { get; set; }

    public string type { get; set; }

    public Content[] contents { get; set; }

    public TextLang description { get; set; }

    public TextLang displayName { get; set; }

    [JsonProperty("@context")]
    public StringList context { get; set; }

    /// <summary>
    /// One or more parent entities
    /// </summary>
    public StringList extends { get; set; }

    public bool Equals(DtdlModel? other)
    {
        return other is DtdlModel m && m.id == id;
    }
}
