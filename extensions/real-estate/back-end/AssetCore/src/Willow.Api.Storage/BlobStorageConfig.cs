using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Api.Storage
{
    public class BlobStorageConfig
    {
        public string AccountName   { get; set; }

        [Obsolete("Use AccountKey")]
        public string Key    { get; set; }

        public string AccountKey    { get; set; }

        public string ContainerName { get; set; }

        [Obsolete("Use ContainerName")]
        public string AssetFileContainer { get; set; }
    }
}
