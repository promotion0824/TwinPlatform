using DTDLParser.Models;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Models
{
    public class InterfaceInfo
    {
        public DTInterfaceInfo Model { get; set; }
        public List<InterfaceInfo> Children { get; set; }

        public string DisplayName => Model.DisplayName.Values.FirstOrDefault();
    }
}