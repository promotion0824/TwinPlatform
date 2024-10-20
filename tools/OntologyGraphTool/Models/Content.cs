
using Newtonsoft.Json;

namespace OntologyGraphTool.Models;

public class Content
{
    [JsonProperty("@type")]
    public StringList type { get; set; }
    public TextLang description { get; set; }
    public TextLang displayName { get; set; }
    public string name { get; set; }
    public Schema schema { get; set; }
    public bool writable { get; set; }

    public string target { get; set; }

    public int minMultiplicity { get; set; }
}

