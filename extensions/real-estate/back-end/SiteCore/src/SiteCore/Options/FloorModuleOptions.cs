using System.Collections.Generic;

namespace SiteCore.Options
{
    public class FloorModuleOptions
    {
        public Module2DOptions Modules2D { get; set; }

        public Module3DOptions Modules3D { get; set; }
    }

    public class Module2DOptions
    {
        public List<string> AllowedExtensions { get; set; } = new List<string>();

        public int MaxSizeBytes { get; set; }

        public int MaxWidth { get; set; }

        public int MaxHeight { get; set; }
    }

    public class Module3DOptions
    {
        public int MaxSizeBytes { get; set; }
    }
}
