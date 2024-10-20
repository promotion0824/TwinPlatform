using System.Collections.Generic;
using System.Linq;

namespace AssetCoreTwinCreator.Models
{
    public class Asset : BaseAsset
    {
        public int ValidationError { get; set; }
        
        // TODO: The Maintenance Responsibility should be a nullable field on the TES_Asset_Register. Otherwise we are matching by string.
        public string MaintenanceResponsibility => AssetParameters != null ? AssetParameters.FirstOrDefault(x => x.Key == "MaintenanceResponsibility")?.Value?.ToString() : "";
        public IEnumerable<AssetParameter> AssetParameters { get; set; }

        public List<double> Geometry { get; set; } = new List<double>();
    }
}