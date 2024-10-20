using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Api.AzureStorage
{
    public class BlobStorageConfig
    {
        /// <summary>
        /// If connection string is provided then other values are ignored
        /// </summary>        
        public string ConnectionString   { get; set; }

        public string AccountName        { get; set; }
        public string ContainerName      { get; set; }

        /// <summary>
        /// To use managed identities do not configure this value
        /// </summary>        
        public string AccountKey         { get; set; }
    }
}
