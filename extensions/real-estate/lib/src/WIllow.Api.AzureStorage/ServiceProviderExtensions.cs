using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Azure.Identity;

using Willow.Azure.Storage;
using Willow.Common;
using Willow.Logging;

namespace Willow.Api.AzureStorage
{
    public static class ServiceProviderExtensions
    {
        public static IBlobStore CreateBlobStore<T>(this IServiceProvider p, BlobStorageConfig config, string path, bool createContainer = true)
        {
            var logger = p.GetService<ILogger<T>>();

            return config.CreateBlobStore(path, logger, createContainer);
        }
    }
}
