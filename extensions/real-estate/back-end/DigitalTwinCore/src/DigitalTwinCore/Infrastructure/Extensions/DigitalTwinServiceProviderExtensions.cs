using System;
using System.Collections.Generic;
using System.Linq;
using DTDLParser;
using DigitalTwinCore.Services;

namespace DigitalTwinCore.Infrastructure.Extensions
{
    public static class DigitalTwinServiceProviderExtensions
    {
		/// <summary>
		/// Get Twin model ids.
		/// </summary>
		/// <param name="provider">DigitalTwinService or CachelessAdtService</param>
		/// <param name="siteIds">List of site ids to lookup</param>
		/// <returns>A key value pair of site id and model ids base on specified parameter</returns>
		public static IAsyncEnumerable<KeyValuePair<Guid, IEnumerable<string>>> GetModelIds(this IDigitalTwinServiceProvider provider, IEnumerable<Guid> siteIds)
        {
            return GetModelIds(provider, siteIds, Array.Empty<Guid>(), string.Empty);
		}

		/// <summary>
		/// Get Twin model ids. It first retrieves a list of model IDs from various site settings, gets the descendants of those models,
		/// and returns a key-value pair of the site ID and the list of descendants for the models. It can filter the descendants by
		/// category IDs or a specific model ID, and can include non-asset models if the includeNonAssetModelsToDefault parameter is set to true.
		/// </summary>
		/// <param name="provider">DigitalTwinService or CachelessAdtService</param>
		/// <param name="siteIds">List of site ids to lookup</param>
		/// <param name="categoryIds">List of Twin category. UniqueId generated from the DTDL modelId. e.g. fe8f5743-a382-7ffa-9213-70e5e0d493f6</param>
		/// <param name="modelId">Specific modelId to return</param>
		/// <param name="includeNonAssetModelsToDefault">Determine if its only return assets or non-assets model ids to default Twin model ids</param>
		/// <returns>A key value pair of site id and model ids</returns>
		public static async IAsyncEnumerable<KeyValuePair<Guid, IEnumerable<string>>> GetModelIds(
			this IDigitalTwinServiceProvider provider,
			IEnumerable<Guid> siteIds,
			Guid[] categoryIds,
			string modelId,
			bool includeNonAssetModelsToDefault = false)
        {
            foreach (var siteId in siteIds)
            {
                var service = await provider.GetForSiteAsync(siteId);
                var modelParser = await service.GetModelParserAsync();

                var siteSettings = service.SiteAdtSettings;

                var modelIds = siteSettings.AssetModelIds.ToList();
                modelIds.AddRange(siteSettings.BuildingComponentModelIds);
                modelIds.AddRange(siteSettings.SpaceModelIds);
                modelIds.AddRange(siteSettings.StructureModelIds);
                modelIds.AddRange(siteSettings.ComponentModelIds);
                modelIds.AddRange(siteSettings.CollectionModelIds);

				if (includeNonAssetModelsToDefault)
				{
					modelIds.AddRange(siteSettings.AccountModelIds);
				}

				var models = modelParser.GetInterfaceDescendants(modelIds.ToArray());

                if (categoryIds?.Any() == true)
                {
                    var ids = new List<string>();
                    foreach (var id in categoryIds)
                    {
                        var categoryDtmi = models.Single(i => i.Value.GetUniqueId() == id).Value.Id.ToString();
                        ids.AddRange(modelParser.GetInterfaceDescendants(new[] { categoryDtmi }).Keys);
                    }
                    yield return new KeyValuePair<Guid, IEnumerable<string>>(siteId, ids);
                }
                else if (!string.IsNullOrEmpty(modelId))
                {
                    var ids = modelParser.GetInterfaceDescendants(new[] { modelId }).Keys;

                    yield return new KeyValuePair<Guid, IEnumerable<string>>(siteId, ids);
                }
                else
                {
                    yield return new KeyValuePair<Guid, IEnumerable<string>>(siteId, models.Keys);
                }
            }
        }
    }
}
