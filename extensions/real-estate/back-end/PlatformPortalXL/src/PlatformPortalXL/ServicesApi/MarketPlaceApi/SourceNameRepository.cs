using PlatformPortalXL.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Willow.Api.Client;
using Willow.Data;

namespace PlatformPortalXL.ServicesApi.MarketPlaceApi
{
    public class SourceNameRepository : IReadRepository<(InsightSourceType SourceType, Guid? SourceId), string>
    {
        private readonly IRestApi _marketPlaceApi;

        public SourceNameRepository(IRestApi marketplaceApi)
        {
            _marketPlaceApi = marketplaceApi;
        }

        public async Task<string> Get((InsightSourceType SourceType, Guid? SourceId) id)
        {
            var sourceName = $"{id.SourceType}";

            if (id.SourceType == InsightSourceType.App)
            {
                if (id.SourceId.HasValue && id.SourceId != Guid.Empty)
                {
                    try
                    {
                        sourceName = (await _marketPlaceApi.Get<App>($"apps/{id.SourceId}")).Name;
                    }
                    catch 
                    {
                        sourceName = id.SourceId.ToString();
                    }
                }
                else
                {
                    sourceName = string.Empty;
                }
            }

            return sourceName;
        }

        public async IAsyncEnumerable<string> Get(IEnumerable<(InsightSourceType SourceType, Guid? SourceId)> ids)
        {
            foreach (var id in ids)
            {
                yield return await Get(id);
            }
        }
    }
}
