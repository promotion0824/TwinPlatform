using Azure.DigitalTwins.Core;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Dto;
using DigitalTwinCore.DTO;
using DigitalTwinCore.Exceptions;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Infrastructure;
using DigitalTwinCore.Models;
using DigitalTwinCore.Serialization;
using DigitalTwinCore.Services.AdtApi;
using DigitalTwinCore.Services.Adx;
using DigitalTwinCore.Services.Query;
using DTDLParser;
using DTDLParser.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.Infrastructure.Exceptions;

namespace DigitalTwinCore.Services.Cacheless
{
	public class CachelessAdtService : IDigitalTwinService
	{
		private const string SiteIdProperty = Properties.SiteId;
		private const string UniqueIdProperty = Properties.UniqueId;
		private const string ScopeTreeCacheKey = "scope-tree";
		private readonly IAdxHelper _adxHelper;
		protected readonly IAdtApiService _adtApiService;
		private readonly ILogger<IDigitalTwinService> _logger;
		private IDictionary<string, AdtModel> _models;
		private IDigitalTwinModelParser _digitalTwinModelParser;
		private IMemoryCache _memoryCache;
        private readonly TimedLock _modelParserLockGuard = new TimedLock();
		private readonly int _maxTwinsAllowedToQuery;
		private const int MaxTwinsAllowedToQueryDefault = 50;

		public CachelessAdtService(
			IAdtApiService adtApiService,
			ILogger<CachelessAdtService> logger,
			IAdxHelper adxHelper,
			IMemoryCache memoryCache,
			IConfiguration configuration = null)
		{
			_adtApiService = adtApiService;
			_logger = logger;
			_adxHelper = adxHelper;
			_memoryCache = memoryCache;

            _maxTwinsAllowedToQuery = configuration.GetValue<int>("MaxTwinsAllowedToQuery", MaxTwinsAllowedToQueryDefault);
		}

		public SiteAdtSettings SiteAdtSettings { get; set; }

		private AzureDigitalTwinsSettings InstanceSettings => SiteAdtSettings.InstanceSettings;

		#region Models
		public Task Load(SiteAdtSettings settings, IMemoryCache memoryCache)
		{
			SiteAdtSettings = settings;

			_models = memoryCache.GetOrCreate($"ModelsCache_{SiteAdtSettings.InstanceSettings.InstanceUri}", (c) =>
			{
				c.SetPriority(CacheItemPriority.NeverRemove);
				c.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

				return _adtApiService.GetModels(InstanceSettings).ToDictionary(x => x.Id, x => x);
			});

			return Task.CompletedTask;
		}

		public async Task<IDigitalTwinModelParser> GetModelParserAsync()
		{
			if (_digitalTwinModelParser == null)
			{
				try
				{
					using (await _modelParserLockGuard.Lock())
					{
						if (_digitalTwinModelParser == null)
						{
							_digitalTwinModelParser = await DigitalTwinModelParser.CreateAsync(GetModels(), _logger);
						}
					}
				}
				catch (ParsingException ex)
				{
					throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Error parsing models", ex);
				}
			}

			return _digitalTwinModelParser;
		}

		public Dictionary<string, List<string>> GetLatestExecutedQueries()
		{
			return _adtApiService.GetLatestExecutedQueries();
		}

		public IList<Model> GetModels()
		{
			return Model.MapFrom(_models.Select(x => x.Value)).ToList();
		}

		public Model GetModel(string id)
		{
			if (!_models.ContainsKey(id))
				return null;

			return Model.MapFrom(_models[id]);
		}

		public IEnumerable<string> GetModelIdsByQuery(string query)
		{
			return _models.Where(x => x.Key.Contains(query, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.Key);
		}

		public async Task<Model> AddModel(string modelJson)
		{
			var dto = await _adtApiService.CreateModel(InstanceSettings, modelJson);
			dto = await _adtApiService.GetModel(InstanceSettings, dto.Id);

			_digitalTwinModelParser = null;

			var model = Model.MapFrom(dto);

			model.ModelDefinition = modelJson;

			_models.TryAdd(dto.Id, dto);

			try
			{
				await _adxHelper.AppendModel(this, dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding model '{Model}' to ADX", model.Id);
			}

			return model;
		}

		public async Task DeleteModel(string id)
		{
			AdtModel model;

			try
			{
				model = await _adtApiService.GetModel(InstanceSettings, id);
			}
			catch (AdtApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return;
			}

			await _adtApiService.DeleteModel(InstanceSettings, id);

			_digitalTwinModelParser = null;

			try
			{
				await _adxHelper.AppendModel(this, model, true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding/deleting model {Model} in ADX", model.Id);
			}

			_models.Remove(id);
		}

		/// <summary>
		/// Return model properties includung inherited.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public async Task<Dictionary<string, BasicModelDto>> GetModelProps(string id)
		{
			var modelParser = await GetModelParserAsync();
			var interfaceInfo = modelParser.GetInterface(id);
			var modelProps = interfaceInfo.Contents
				.Where(entry => entry.Value.EntityKind == DTEntityKind.Property)
				.ToDictionary(e => e.Key, e => new BasicModelDto
				{
					DisplayName = e.Value.DisplayName,
					Schema = (e.Value as DTPropertyInfo).Schema
				});

			return modelProps;
		}
		#endregion

		#region Twins
		public async Task<Page<Twin>> GetTwinsAsync(IEnumerable<string> twinIds = null, string continuationToken = null)
		{
			var queryBuilder = AdtQueryBuilder.Create()
				.SelectAll()
				.FromDigitalTwins();
			var query = queryBuilder.GetQuery();

			if (twinIds != null && twinIds.Any())
			{
				query = queryBuilder
					.Where()
					.WithPropertyIn("$dtId", twinIds)
					.GetQuery();
			}

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);

			var page = await pageable.AsPages(continuationToken).FirstAsync();

			return new Page<Twin> { ContinuationToken = page.ContinuationToken, Content = page.Values.Select(x => Twin.MapFrom(x)).ToList() };
		}

		public async Task<Page<Twin>> GetTwinsByModelsAsync(Guid siteId, IEnumerable<string> models, bool restrictToSite, string continuationToken = null)
		{
			IAdtQueryBuilder queryBuilder = AdtQueryBuilder.Create()
				.SelectAll()
				.FromDigitalTwins();

			if ((models != null && models.Any()) || restrictToSite)
				queryBuilder = (queryBuilder as IAdtQueryWhere).Where();

			if (restrictToSite)
				queryBuilder = (queryBuilder as IAdtQueryFilterGroup).WithStringProperty("siteID", siteId.ToString());

			if (models != null && models.Any())
			{
				if (restrictToSite)
					queryBuilder = (queryBuilder as IAdtQueryFilterGroup).And();

				queryBuilder = (queryBuilder as IAdtQueryFilterGroup)
					.WithAnyModel(models);
			}

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, (queryBuilder as IAdtQueryFilterGroup).GetQuery());

			var page = await pageable.AsPages(continuationToken).FirstAsync();

			return new Page<Twin> { ContinuationToken = page.ContinuationToken, Content = page.Values.Select(x => Twin.MapFrom(x)).ToList() };
		}

		public async Task<Page<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(string continuationToken, IEnumerable<string> twinIds = null)
		{
			var twins = await GetTwinsAsync(twinIds, continuationToken);
			var resultPage = new Page<TwinWithRelationships> { ContinuationToken = twins.ContinuationToken, Content = twins.Content.Select(x => new TwinWithRelationships { Id = x.Id, CustomProperties = x.CustomProperties, Metadata = x.Metadata }).ToList() };

			if (!twins.Content.Any())
				return resultPage;

			var relationshipsMap = new ConcurrentDictionary<string, List<TwinRelationship>>(resultPage.Content.ToDictionary(x => x.Id, x => new List<TwinRelationship>()));

			var getRelationships = twins.Content.Select(async x =>
			{

				var relationships = await _adtApiService.GetRelationships(InstanceSettings, x.Id);

				relationships.ToList().ForEach(r =>
				{
					var twinRelationship = new TwinRelationship
					{
						Id = r.Id,
						Name = r.Name,
						Source = resultPage.Content.First(x => x.Id == r.SourceId),
						Target = GetTwinByIdAsync(r.TargetId, false).Result,
						CustomProperties = r.Properties
					};
					relationshipsMap[r.SourceId].Add(twinRelationship);
				});
			});

			await Task.WhenAll(getRelationships);

			resultPage.Content = resultPage.Content.Select(x =>
			{
				x.Relationships = relationshipsMap[x.Id];
				return x;
			});

			return resultPage;
		}

		public async Task<IEnumerable<TwinWithRelationships>> GetTwinsWithRelationshipsAsync(Guid siteId, IEnumerable<string> twinIds = null)
		{
			var query = (AdxQueryBuilder.Create()
				.Select(AdxConstants.ActiveTwinsFunction) as IAdxQuerySelector);

			if (twinIds != null && twinIds.Any())
			{
				(query as IAdxQueryWhere).Where().PropertyIn("Id", twinIds);
			}

			query.Join(
					AdxQueryBuilder.Create()
						.Select(AdxConstants.ActiveRelationshipsFunction)
						.GetQuery(),
					"Id",
					"SourceId",
					"leftouter")
				.Summarize()
				.SetProperty("Relationships").MakeSet(true, "Raw1")
				.TakeAny(false, "Raw")
				.By("Id");

			using var reader = await _adxHelper.Query(this, query.GetQuery());
			var twinsWithRelationships = new List<TwinWithRelationships>();

			while (reader.Read())
			{
				var twin = JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter());
				var twinWithRelationships = new TwinWithRelationships { Id = twin.Id, CustomProperties = twin.CustomProperties, Metadata = twin.Metadata };

				var relationshipsArray = reader["Relationships"] as JArray;
				if (relationshipsArray != null && relationshipsArray.HasValues)
				{
					var relationships = relationshipsArray.Select(x =>
					{
						var basicRelationship = JsonConvert.DeserializeObject<BasicRelationship>(x.ToString());

						return new TwinRelationship
						{
							Id = basicRelationship.Id,
							Name = basicRelationship.Name,
							Target = new TwinWithRelationships { Id = basicRelationship.TargetId },
							Source = new TwinWithRelationships { Id = basicRelationship.SourceId },
							CustomProperties = Twin.MapCustomProperties(basicRelationship.Properties)
						};
					}
					);
					twinWithRelationships.Relationships = relationships.ToList().AsReadOnly();
				}
				twinsWithRelationships.Add(twinWithRelationships);
			}


			return twinsWithRelationships;
		}

		public async Task<TwinWithRelationships> PatchTwin(string id, JsonPatchDocument patch, Azure.ETag? ifMatch, string userId)
		{
			var twin = await GetTwinByIdAsync(id);
			if (twin == null)
			{
				throw new ResourceNotFoundException("twin", id);
			}

			EnsureTwinAccess(twin);

			await _adtApiService.PatchTwin(InstanceSettings, id, patch, ifMatch);

			var twinForAdxUpdate = await GetTwinFromAdt(InstanceSettings, id);
			await _adxHelper.AppendTwin(this, twinForAdxUpdate, false, userId);

			return await GetTwinByIdAsync(id, true);
		}

		public async Task<List<TwinWithRelationships>> GetTwinsByQueryAsync(string query, string alias = null)
		{
			var dtos = await _adtApiService.GetTwins(InstanceSettings, query);
			if (alias != null)
			{
				dtos = dtos.Select(dto => dto.GetValue(alias)).ToList();
			}

			var twinsWithRelationships = dtos.Select(d =>
				new TwinWithRelationships
				{
					Id = d.Id,
					CustomProperties = Twin.MapCustomProperties(d.Contents),
					Metadata = TwinMetadata.MapFrom(d.Metadata)
				}).ToList();

			return twinsWithRelationships;
		}

		public async Task<Page<TwinWithRelationships>> GetSiteTwinsAsync(string continuationToken = null)
		{
			var models = new List<string>(SiteAdtSettings.AssetModelIds);
			models.AddRange(SiteAdtSettings.SpaceModelIds);
			var query = AdtQueryBuilder.Create()
				.SelectAll()
				.FromDigitalTwins()
				.Where()
				.WithAnyModel(models)
				.And()
				.OpenGroupParenthesis()
				.WithStringProperty("siteID", SiteAdtSettings.SiteId.ToString())
				.Or()
				.WithStringProperty("siteID", Guid.Empty.ToString())
				.Or()
				.Not()
				.IsDefined("siteID")
				.CloseGroupParenthesis()
				.GetQuery();

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);

			var page = await pageable.AsPages(continuationToken).FirstAsync();

			return new Page<TwinWithRelationships> { ContinuationToken = page.ContinuationToken, Content = page.Values.Select(x => TwinWithRelationships.MapFrom(x)).ToList() };
		}

		public async Task<Page<TwinWithRelationships>> GetFloorTwinsAsync(Guid floorId, string continuationToken = null)
		{
			var floorQuery = AdtQueryBuilder.Create()
				.SelectSingle()
				.FromDigitalTwins()
				.Where()
				.WithStringProperty("uniqueID", floorId.ToString())
				.And()
				.CheckDefined(SiteAdtSettings.LevelModelIds.ToList())
				.GetQuery();
			var floors = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, floorQuery);
			var floor = await floors.SingleAsync();

			var models = new List<string>(SiteAdtSettings.AssetModelIds);
			models.AddRange(SiteAdtSettings.BuildingComponentModelIds);
			models.AddRange(SiteAdtSettings.SpaceModelIds);
			var query = AdtQueryBuilder.Create()
				.Select("Asset")
				.FromDigitalTwins()
				.Match(new string[] { "locatedIn", "isPartOf" }, "Asset", "Floor", "*1..9")
				.Where()
				.WithStringProperty("Floor.$dtId", floor.Id)
				.And()
				.WithAnyModel(models, "Asset")
				.GetQuery();

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);

			var page = await pageable.AsPages(continuationToken).FirstAsync();

			return new Page<TwinWithRelationships>
			{
				ContinuationToken = page.ContinuationToken,
				Content = page.Values
					.Select(x => TwinWithRelationships.MapFrom(x.GetValue("Asset")))
					.ToList()
			};
		}

        public async Task<Page<TwinWithRelationships>> GetTwinsByModuleTypeNameAsync(List<string> requestModuleTypeNamePaths,
            string continuationToken=null)
        {
            var query = AdtQueryBuilder.Create()
                .SelectAll()
                .FromDigitalTwins()
                .Where()
                .WithStringProperty("siteID", SiteAdtSettings.SiteId.ToString())
                .And()
                .WithPropertyIn(Properties.GeometrySpatialReference, requestModuleTypeNamePaths)
                .GetQuery();
            var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);
            var page = await pageable.AsPages(continuationToken).FirstAsync();

            return new Page<TwinWithRelationships> { ContinuationToken = page.ContinuationToken, Content = page.Values.Select(x => TwinWithRelationships.MapFrom(x)).ToList() };
        }
        public async Task<TwinWithRelationships> GetTwinByExternalIdAsync(string externalId)
		{
			var query = AdtQueryBuilder.Create()
				.SelectSingle()
				.FromDigitalTwins()
				.Where()
				.WithStringProperty("externalID", externalId)
				.GetQuery();

			var basicDigitalTwin = await _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query).SingleOrDefaultAsync();

			if (basicDigitalTwin == null)
			{
				return null;
			}

			var twin = new TwinWithRelationships
			{
				Id = basicDigitalTwin.Id,
				CustomProperties = Twin.MapCustomProperties(basicDigitalTwin.Contents),
				Metadata = TwinMetadata.MapFrom(basicDigitalTwin.Metadata)
			};
			EnsureTwinAccess(twin);

			await AppendRelationshipsAsync(twin);

			return twin;
		}

		public async Task<TwinWithRelationships> GetTwinByForgeViewerIdAsync(string id)
		{
			var query = AdtQueryBuilder.Create()
				.SelectSingle()
				.FromDigitalTwins()
				.Where()
				.WithStringProperty("geometryViewerID", id)
				.GetQuery();

			var twin = await _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query).SingleOrDefaultAsync();

			if (twin == null)
				return null;

			return new TwinWithRelationships
			{
				Id = twin.Id,
				CustomProperties = Twin.MapCustomProperties(twin.Contents),
				Metadata = TwinMetadata.MapFrom(twin.Metadata)
			};
		}

		public async Task<Guid?> GetRelatedSiteId(string twinId)
		{
			var query = AdtQueryBuilder.Create()
				.Select("Target.siteID")
				.FromDigitalTwins()
				.Match(new string[] { Relationships.LocatedIn, Relationships.IsPartOf, Relationships.IsCapabilityOf, Relationships.IsDocumentOf, Relationships.HostedBy }, "Source", "Target", "*1..3", targetDirection: "-")
				.Where()
				.IsDefined("Target.siteID")
				.And()
				.WithStringProperty("Source.$dtId", twinId)
				.GetQuery();

			var pageable = _adtApiService.QueryTwins<Dictionary<string, string>>(InstanceSettings, query);
			var page = await pageable.AsPages().FirstAsync();

			var twinResponse = page?.Values.FirstOrDefault();
			if (twinResponse is null) return null;

			return Guid.TryParse(twinResponse["siteID"], out Guid siteId) ? siteId : (Guid?)null;
		}

		public async Task<Twin> GetTwinFloor(string twinId)
		{
			_logger.LogTrace("Finding floor for twin {TwinId}.", twinId);

			try
			{
				var query = AdtQueryBuilder.Create()
				.Select("Floor")
				.FromDigitalTwins()
				.Match(new string[] { Relationships.LocatedIn, Relationships.IsPartOf }, "Asset", "Floor", "*..3")
				.Where()
				.WithStringProperty("Asset.$dtId", twinId)
				.And()
				.WithAnyModel(SiteAdtSettings.LevelModelIds, "Floor")
				.GetQuery();

				var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);
				var page = await pageable.AsPages().FirstAsync();

				var twinResponse = page?.Values.FirstOrDefault();
				if (twinResponse is null)
				{
					_logger.LogWarning("Cannot find floor of twin {TwinId}.", twinId);
					return null;
				}

				var floorTwin = System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwin>(twinResponse.Contents.First().Value.ToString());

				return new TwinWithRelationships
				{
					Id = floorTwin.Id,
					CustomProperties = Twin.MapCustomProperties(floorTwin.Contents),
					Metadata = TwinMetadata.MapFrom(floorTwin.Metadata)
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cannot get floor of the twin {TwinId}.", twinId);
				return null;
			}
		}

		private async Task<BasicDigitalTwin> GetTwinFromAdt(AzureDigitalTwinsSettings settings, string twinId)
		{
			BasicDigitalTwin twin = null;

			try
			{
				twin = await _adtApiService.GetTwin(settings, twinId);
			}
			catch (AdtApiException exception) when (exception.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				twin = null;
			}
			return twin;
		}

		public async Task<TwinWithRelationships> GetTwinByIdAsync(string id, bool loadRelationships = true)
		{
			var basicDigitalTwin = await GetTwinFromAdt(InstanceSettings, id);
			if (basicDigitalTwin == null)
			{
				return null;
			}

			var twinWithRelationships = new TwinWithRelationships
			{
				Id = basicDigitalTwin.Id,
				CustomProperties = Twin.MapCustomProperties(basicDigitalTwin.Contents),
				Metadata = TwinMetadata.MapFrom(basicDigitalTwin.Metadata),
				Etag = basicDigitalTwin.ETag.GetValueOrDefault().ToString()
			};

			EnsureTwinAccess(twinWithRelationships);

			if (loadRelationships)
				await AppendRelationshipsAsync(twinWithRelationships);

			return twinWithRelationships;
		}

		public async Task AppendRelationshipsAsync(TwinWithRelationships twinWithRelationships)
		{
			try
			{
				var twinRelationships = await _adtApiService.GetRelationships(InstanceSettings, twinWithRelationships.Id);
				var loadedRelationships = new ConcurrentBag<TwinRelationship>();

				var loadRelationshipsAction = twinRelationships.Select(async x =>
					loadedRelationships.Add(new TwinRelationship
					{
						Id = x.Id,
						Name = x.Name,
						Source = twinWithRelationships,
						Target = await GetTwinByIdAsync(x.TargetId, false),
						CustomProperties = x.Properties
					}));
				await Task.WhenAll(loadRelationshipsAction);

				twinWithRelationships.Relationships = loadedRelationships.ToList().AsReadOnly();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Cannot append relationships to {TwinWithRelationshipsId}.", twinWithRelationships.Id);
			}
		}

		// TODO: remove method when cacheless is stable.
		public Task<TwinWithRelationships> GetTwinByIdUncachedAsync(string id)
		{
			return GetTwinByIdAsync(id);
		}

		public async Task<TwinWithRelationships> GetTwinByTrendIdAsync(Guid uniqueId)
		{
			var query = AdtQueryBuilder.Create()
				   .SelectSingle()
				   .FromDigitalTwins()
				   .Where()
				   .WithStringProperty("trendID", uniqueId.ToString())
				   .GetQuery();

			var twin = await _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query).SingleOrDefaultAsync();

			if (twin == null)
			{
				return null;
			}

			var twinWithRelationships = new TwinWithRelationships
			{
				Id = twin.Id,
				CustomProperties = Twin.MapCustomProperties(twin.Contents),
				Metadata = TwinMetadata.MapFrom(twin.Metadata)
			};
			EnsureTwinAccess(twinWithRelationships);

			await AppendRelationshipsAsync(twinWithRelationships);

			return twinWithRelationships;
		}

		public async Task<TwinWithRelationships> GetTwinByUniqueIdAsync(Guid id, bool hasAccessToSharedTwin = false)
		{
			var query = AdtQueryBuilder.Create()
				.SelectSingle()
				.FromDigitalTwins()
				.Where()
				.WithStringProperty("uniqueID", id.ToString())
				.GetQuery();

			var twin = await _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query).SingleOrDefaultAsync();

			if (twin == null)
				return null;

			return new TwinWithRelationships
			{
				Id = twin.Id,
				CustomProperties = Twin.MapCustomProperties(twin.Contents),
				Metadata = TwinMetadata.MapFrom(twin.Metadata)
			};
		}

		public async Task<List<TwinIdDto>> GetTwinIdsByUniqueIdsAsync(List<Guid> uniqueIds)
		{
			var result = new List<TwinIdDto>();
			if (uniqueIds == null || !uniqueIds.Any())
			{
				return result;
			}
			var query = (AdxQueryBuilder.Create()
					.Select(AdxConstants.ActiveTwinsFunction).Where()
					.PropertyIn("UniqueId", uniqueIds.Select(c => c.ToString())) as IAdxQuerySelector)
				.ProjectKeep("Id", "UniqueId").GetQuery();

			using var reader = await _adxHelper.Query(this, query);
			while (reader.Read())
			{
				result.Add(new TwinIdDto
				{
					Id = reader["Id"].ToString(),
					UniqueId = reader["UniqueId"].ToString()
				});
			}
			return result;

		}

		public async Task<TwinWithRelationships> AddOrUpdateTwinAsync(Twin entity, bool isSyncRequired = true, string userId = null)
		{
			var parser = await GetModelParserAsync();
			var interfaceInfo = parser.GetInterface(entity.Metadata.ModelId);

			if (interfaceInfo.Contents.ContainsKey(SiteIdProperty) && !entity.CustomProperties.ContainsKey(SiteIdProperty))
			{
				entity.CustomProperties.Add(SiteIdProperty, SiteAdtSettings.SiteId);
			}

			// Note that currently all top-level models have this property, so the test below is always true
			// TODO: Handle all immutable properties such as uniqueID in a consistent way, or investigate as ADT feature
			if (interfaceInfo.Contents.ContainsKey(UniqueIdProperty))
			{
				var entityUniqId = entity.UniqueIdFromProperties;
				var existingWithTwinId = await GetTwinByIdAsync(entity.Id);
				if (existingWithTwinId != null && entityUniqId != null && entityUniqId != existingWithTwinId.UniqueId)
				{
					throw new DigitalTwinCoreException(SiteAdtSettings.SiteId,
						$"Can't update twin {entity.Id} with uniqueId: {entityUniqId} with the new uniqueID {existingWithTwinId.UniqueId}");
				}
				else if (existingWithTwinId == null && entityUniqId != null)
				{
					var existingWithUniqId = await GetTwinByUniqueIdAsync(entityUniqId.Value);
					if (existingWithUniqId != null && entity.Id != existingWithUniqId.Id)
					{
						throw new DigitalTwinCoreException(SiteAdtSettings.SiteId,
							$"Can't create twin {entity.Id} with uniqueID {entityUniqId}- the twin {existingWithUniqId.Id} with the same uniqueID already exists");
					}
				}

				if (entityUniqId == null)
				{
					// Add a uniqueId if none has been provided -- make sure to re-use any existing uniqueId
					try
					{
						entity.CustomProperties.Add(UniqueIdProperty, existingWithTwinId?.UniqueId ?? Guid.NewGuid());
					}
					catch (DigitalTwinCoreException)
					{
						entity.CustomProperties.Add(UniqueIdProperty, Guid.NewGuid());
					}
				}
			}

			var providedSiteId = entity.GetStringProperty(SiteIdProperty);
			if (providedSiteId != null && providedSiteId != SiteAdtSettings.SiteId.ToString())
			{
				throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Failed to add twin. Site Id in twin JSON must match the site id in the request URI.");
			}

			EnsureTwinAccess(entity);

			var dto = entity.MapToDto();

			var dtoResponse = await _adtApiService.AddOrUpdateTwin(InstanceSettings, entity.Id, dto);

			var twinWithRelationships = new TwinWithRelationships
			{
				Id = dtoResponse.Id,
				CustomProperties = Twin.MapCustomProperties(dtoResponse.Contents),
				Metadata = TwinMetadata.MapFrom(dtoResponse.Metadata)
			};

			await AppendRelationshipsAsync(twinWithRelationships);

			try
			{
				if (isSyncRequired)
					await _adxHelper.AppendTwin(this, dtoResponse, false, userId);
				else
					_logger.LogInformation("ADX sync skipped for twin {Id}", dto.Id);

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding twin {Id} to ADX", dtoResponse.Id);
			}

			return twinWithRelationships;
		}

		public async Task DeleteTwinsAndRelationshipsAsync(Guid siteId, IEnumerable<string> ids)
		{
			var relationships = new ConcurrentBag<Tuple<string, string>>();

			var getRelationshipIds = ids.Select(async x =>
			{
				var incoming = await _adtApiService.GetIncomingRelationships(InstanceSettings, x);
				var outgoing = await _adtApiService.GetRelationships(InstanceSettings, x);

				incoming.ToList().ForEach(r => relationships.Add(new Tuple<string, string>(x, r.RelationshipId)));
				outgoing.ToList().ForEach(r => relationships.Add(new Tuple<string, string>(x, r.Id)));
			});

			await Task.WhenAll(getRelationshipIds);

			var deleteRelationships = relationships.Select(async x => await DeleteRelationshipAsync(x.Item1, x.Item2));

			await Task.WhenAll(deleteRelationships);

			var deleteTwins = ids.Select(async x => await DeleteTwinAsync(x));

			await Task.WhenAll(deleteTwins);
		}

		public async Task DeleteTwinAsync(string id)
		{
			var twin = await GetTwinFromAdt(InstanceSettings, id);

			if (twin == null)
				return;

			EnsureTwinAccess(Twin.MapFrom(twin));

			await _adtApiService.DeleteTwin(InstanceSettings, id);

			try
			{
				await _adxHelper.AppendTwin(this, twin, true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding twin {Id} to ADX", twin.Id);
			}
		}

		public async Task DeleteTwinAndRelationshipsAsync(string id)
		{
			await DeleteTwinsAndRelationshipsAsync(Guid.NewGuid(), Enumerable.Repeat(id, 1));
		}

		/// <summary>
		/// Generic method to fetch relationships using ADT query based on twinId, twinmodels, and condition of the relationship including names, direction and number of hops
		/// </summary>
		/// <param name="twinId">Twin's dtId</param>
		/// <param name="relNames">relationship names between the twins</param>
		/// <param name="targetModels">models that target twins should be falls into</param>
		/// <param name="hops">max number of hops for the relationships between twins</param>
		/// <param name="sourceDirection">right-to-left direction of relationship e.g. "<-"</param>
		/// <param name="targetDirection">left-to-right direction of relationship e.g. "->"</param>
		/// <returns></returns>
		public async Task<List<TwinRelationship>> GetTwinRelationshipsByQuery(string twinId, string[] relNames, string[] targetModels, int hops, string sourceDirection, string targetDirection)
		{
			var queryBuilder = AdtQueryBuilder.Create()
			.Select("TargetTwin")
			.FromDigitalTwins()
			.Match(relNames, "SourceTwin", "TargetTwin", $"*..{hops}", sourceDirection ?? "-", targetDirection ?? "->")
			.Where()
			.WithStringProperty("SourceTwin.$dtId", twinId);

			if (targetModels?.Any() == true)
				queryBuilder.And().WithAnyModel(targetModels, "TargetTwin");

			var query = queryBuilder.GetQuery();

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);
			// Eliminate duplicate twins as multiple hops query could return multiple same twin
			var targetTwins = (await pageable.Select(t => System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwin>
															(t.Contents.First().Value.ToString())).ToListAsync())
																.DistinctBy(t => t.Id);

			var targetRelationships = new List<TwinRelationship>();

			if (!targetTwins.Any())
			{
				_logger.LogWarning("Cannot find specified relationships of twin {TwinId}.", twinId);
				return targetRelationships;
			}

			var targetTwinIds = targetTwins.Select(t => t.Id).ToList();

			foreach (var twin in targetTwins)
			{
				// Note currently ADT query does not support both hops and query variable of relationships.
				// So have to fetch the relationshiop separately
				var targetTwinRelationships = await _adtApiService.GetRelationships(InstanceSettings, twin.Id);
				var inclusiveRelationships = targetTwinRelationships.Where(r => targetTwinIds.Contains(r.TargetId)).ToList();

				targetRelationships.AddRange(
						inclusiveRelationships.Select(
							r => new TwinRelationship
							{
								Id = r.Id,
								Name = r.Name,
								CustomProperties = r.Properties,
								Source = TwinWithRelationships.MapFrom(twin),
								Target = TwinWithRelationships.MapFrom(targetTwins.Single(t => t.Id == r.TargetId))
							}));
			}
			return targetRelationships;
		}

		/// <summary>
		/// Get related twins by specified hops of the twin.
		/// </summary>
		/// <param name="twinId">Twin's dtId</param>
		/// <param name="hops">Number of hops from twins</param>
		/// <returns>Related Twins in the range of hops</returns>
		public async Task<IEnumerable<Twin>> GetRelatedTwinsByHops(string twinId, int hops)
		{
			var queryBuilder = AdtQueryBuilder.Create()
			.Select("Target")
			.FromDigitalTwins()
			.Match(Array.Empty<string>(), "Source", "Target", $"*..{hops}", "-", "-")
			.Where()
			.WithStringProperty("Source.$dtId", twinId);

			var query = queryBuilder.GetQuery();

			var pageable = _adtApiService.QueryTwins<BasicDigitalTwin>(InstanceSettings, query);
			var targetTwins = await pageable.Select(t => System.Text.Json.JsonSerializer.Deserialize<BasicDigitalTwin>(t.Contents.First().Value.ToString())).ToListAsync();

			return Twin.MapFrom(targetTwins);
		}


        
        /// <summary>
        /// Retrieves a list of building twins based on their external IDs.
        /// </summary>
        /// <param name="externalIdValues">The list of external ID values.</param>
        /// <param name="externalIdName">The name of the external ID property.</param>
        /// <returns>A list of building twins matching the provided external IDs.</returns>
        public async Task<List<BuildingsTwinDto>> GetBuildingTwinsByExternalIds(List<string> externalIdValues, string externalIdName)
        {
            externalIdName = externalIdName.Escape();
            externalIdValues = externalIdValues.Select(x => $"'{x.Escape()}'").ToList();

            var externalIdsString = string.Join(",", externalIdValues);

            var query = @$"SELECT  twin.$dtId AS TwinId, twin.externalIds AS ExternalIds FROM DIGITALTWINS twin
                            WHERE IS_OF_MODEL('dtmi:com:willowinc:Building;1') AND twin.externalIds.{externalIdName} IN [{externalIdsString}]";
            var pageable = _adtApiService.QueryTwins<BuildingsTwinDto>(InstanceSettings, query);
            var result = await pageable.ToListAsync();
            return result;          
        }


        private void EnsureTwinAccess(Twin twin)
		{
			// We do not support mixing twins for multiple customers in one ADT instance,
			//   regardless of the value of the siteID property of a twin -- therefore this
			//   check is not necessary. When this check is enabled, is stops the delivery
			//   team from being able to create relationships between portfilio-level twins
			//   which have different siteIDs.  A siteID in the API URI will route us to the corrent
			//   instance for the customer -- after that, we allow any operation regardless of siteID.
#if false
			if (twin != null)
			{
				var twinSiteId = twin.GetSiteId(SiteAdtSettings.SiteModelIds);
				if (twinSiteId != null && twinSiteId != SiteAdtSettings.SiteId)
				{
					throw new DigitalTwinCoreException(SiteAdtSettings.SiteId, "Access to twin denied. Invalid site.");
				}
			}
#endif
		}

		#endregion

		#region Relationships

		public async Task<List<TwinRelationship>> GetRelationships(string twinId)
		{
			List<BasicRelationship> relationships = null;
			try
			{
				relationships = await _adtApiService.GetRelationships(InstanceSettings, twinId);
			}
			catch (AdtApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return null;
			}

			return await GetRelationships(relationships);
		}

		public async Task<List<TwinRelationship>> GetRelationships(IEnumerable<BasicRelationship> relationships)
		{
			if (relationships == null || !relationships.Any())
				return new List<TwinRelationship>();

			// Start source and target tasks in parallel.
			var sourceTasks = new Dictionary<string, Task<BasicDigitalTwin>>();
			var targetTasks = new Dictionary<string, Task<BasicDigitalTwin>>();
			foreach (var rel in relationships)
			{
				sourceTasks.Add(rel.Id, GetTwinFromAdt(InstanceSettings, rel.SourceId));
				targetTasks.Add(rel.Id, GetTwinFromAdt(InstanceSettings, rel.TargetId));
			}

			var sourceResults = await Task.WhenAll(sourceTasks.Select(async p => (p, await p.Value)));
			var targetResults = await Task.WhenAll(targetTasks.Select(async p => (p, await p.Value)));

			return relationships.Select(r =>
				new TwinRelationship
				{
					Id = r.Id,
					Name = r.Name,
					Source = TwinWithRelationships.MapFrom(sourceResults.Single(sr => sr.p.Key == r.Id).Item2),
					Target = TwinWithRelationships.MapFrom(targetResults.Single(tr => tr.p.Key == r.Id).Item2),
					CustomProperties = Twin.MapCustomProperties(r.Properties)
				}).ToList();
		}

		public async Task<List<TwinRelationship>> GetIncomingRelationshipsAsync(string twinId)
		{
			if (_maxTwinsAllowedToQuery <= 0)
				throw new DigitalTwinCoreException(null, "Invalid max twin allowed to query value.");

			//The GetIncomingRelationships method is not returning all the records so using the query to return all the records
			var basicRelationships = _adtApiService.QueryTwins<BasicRelationship>(InstanceSettings, $"SELECT * FROM RELATIONSHIPS WHERE $targetId = '{twinId}'");
			var relationships = new List<BasicRelationship>();
			await foreach (var basicRelationship in basicRelationships)
			{
				relationships.Add(basicRelationship);
			}

			var targetTwin = await GetTwinFromAdt(InstanceSettings, twinId);

			var twinSourceIds = relationships.Select(x => x.SourceId).ToList();
			var sourceTwins = await GetTwinsBulkAsync(twinSourceIds, _maxTwinsAllowedToQuery);

			return relationships.Select(x =>
				new TwinRelationship
				{
					Id = x.Id,
					Name = x.Name,
					CustomProperties = x.Properties,
					Source = sourceTwins.FirstOrDefault(y => y.Id == x.SourceId),
					Target = TwinWithRelationships.MapFrom(targetTwin)
				})
				.ToList();
		}

        public async Task<List<TwinRelationship>> GetTwinRelationshipsAsync(string dtId)
        {
			var basicRelationships = _adtApiService.QueryTwins<BasicRelationship>(
                InstanceSettings,
                $"SELECT * FROM RELATIONSHIPS WHERE $sourceId = '{dtId.Escape()}' OR $targetId = '{dtId.Escape()}'"
            );
			var relationships = new List<BasicRelationship>();
			await foreach (var basicRelationship in basicRelationships)
			{
				relationships.Add(basicRelationship);
			}

			var otherTwinIds = relationships.Select(x => x.SourceId == dtId ? x.TargetId : x.SourceId).Distinct().ToList();
            otherTwinIds.Add(dtId);

			var twins = await GetTwinsBulkAsync(otherTwinIds, _maxTwinsAllowedToQuery);
            var twinsLookup = twins.ToDictionary(t => t.Id, t => t);

			return relationships.Select(x =>
				new TwinRelationship
				{
					Id = x.Id,
					Name = x.Name,
					CustomProperties = x.Properties,
					Source = twinsLookup[x.SourceId],
					Target = twinsLookup[x.TargetId]
				})
				.ToList();
        }

		/// <summary>
		/// Query bulk twins by grouping them up in an allowable array items in an ADT IN clause
		/// https://github.com/MicrosoftDocs/azure-docs/blob/main/includes/digital-twins-limits.md
		/// </summary>
		/// <param name="twinIds">List of twin ids to query</param>
		/// <param name="maxTwinsAllowedToQuery">Maximum twins allowed to query in ADT</param>
		/// <returns></returns>
		private async Task<List<TwinWithRelationships>> GetTwinsBulkAsync(List<string> twinIds, int maxTwinsAllowedToQuery)
		{
			var twins = new List<TwinWithRelationships>();
			var splits = SplitTwinIds(twinIds, maxTwinsAllowedToQuery);
			foreach (var split in splits)
			{
				var pageTwins = await GetTwinsAsync(split);
				twins.AddRange(pageTwins.Content.Select(x =>
					new TwinWithRelationships
					{
						Id = x.Id,
						CustomProperties = x.CustomProperties,
						Metadata = x.Metadata,
						Etag = x.Etag
					})
					.ToList());
			}
			return twins;
		}

		private static List<List<string>> SplitTwinIds(List<string> twins, int size)
		{
			List<List<string>> result = new List<List<string>>();
			for (int i = 0; i < twins.Count; i += size)
			{
				result.Add(twins.GetRange(i, Math.Min(size, twins.Count - i)));
			}
			return result;
		}

		public async Task<IEnumerable<TwinIncomingRelationship>> GetBasicIncomingRelationshipsAsync(string twinId)
		{
			var relationships = await _adtApiService.GetIncomingRelationships(InstanceSettings, twinId);

			return relationships.Select(x => TwinIncomingRelationship.MapFrom(x));
		}

		public async Task<TwinRelationship> GetRelationshipAsync(string twinId, string id)
		{
			var twin = await GetTwinByIdAsync(twinId, false);
			if (twin == null)
			{
				return null;
			}

			EnsureTwinAccess(twin);

			var relationship = await _adtApiService.GetRelationship(InstanceSettings, twinId, id);

			if (relationship == null)
				return null;

			return new TwinRelationship
			{
				Id = relationship.Id,
				Name = relationship.Name,
				Source = twin,
				Target = await GetTwinByIdAsync(relationship.TargetId, false),
				CustomProperties = relationship.Properties
			};
		}

		// TODO: remove method when cacheless is stable.
		public Task<TwinRelationship> GetRelationshipUncachedAsync(string twinId, string id)
		{
			return GetRelationshipAsync(twinId, id);
		}

		public async Task<TwinRelationship> AddRelationshipAsync(string twinId, string id, Relationship relationship)
		{
			var twin = await GetTwinByIdAsync(twinId, false);
			if (twin == null)
			{
				throw new ResourceNotFoundException("Source Twin", twinId);
			}
			EnsureTwinAccess(twin);

			var targetTwin = await GetTwinByIdAsync(relationship.TargetId, false);
			if (targetTwin == null)
			{
				throw new ResourceNotFoundException("Target Twin", relationship.TargetId);
			}
			EnsureTwinAccess(targetTwin);

			var dtoResponse = await _adtApiService.AddRelationship(InstanceSettings, twinId, id, relationship.MapToDto());

			try
			{
				await _adxHelper.AppendRelationship(this, dtoResponse);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding relationship {Id} to ADX", dtoResponse.Id);
			}

			return new TwinRelationship
			{
				Id = dtoResponse.Id,
				Name = dtoResponse.Name,
				Source = twin,
				Target = targetTwin,
				CustomProperties = dtoResponse.Properties
			};
		}

		public async Task<IReadOnlyCollection<Twin>> GetTwinsAsync(Guid siteId)
		{
			var query = (AdxQueryBuilder.Create()
				.Select(AdxConstants.ActiveTwinsFunction) as IAdxQuerySelector).Project("Raw");

			var twins = new List<Twin>();
			using var reader = await _adxHelper.Query(this, query.GetQuery());

			while (reader.Read())
			{
				twins.Add(JsonConvert.DeserializeObject<Twin>(reader["Raw"].ToString(), new TwinJsonConverter()));
			}

			return twins;
		}

		public async Task<TwinRelationship> UpdateRelationshipAsync(string twinId, string id, JsonPatchDocument value)
		{
			var twin = await GetTwinByIdAsync(id, false);
			if (twin == null)
			{
				return null;
			}
			EnsureTwinAccess(twin);

			var dto = await _adtApiService.UpdateRelationship(InstanceSettings, twinId, id, value);

			try
			{
				await _adxHelper.AppendRelationship(this, dto);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding relationship {Id} to ADX", dto.Id);
			}

			return new TwinRelationship
			{
				Id = dto.Id,
				Name = dto.Name,
				Source = twin,
				Target = await GetTwinByIdAsync(dto.TargetId, false),
				CustomProperties = dto.Properties
			};
		}

		public async Task DeleteRelationshipAsync(string twinId, string id)
		{
			var twin = await GetTwinByIdAsync(twinId, false);
			if (twin == null)
			{
				throw new ResourceNotFoundException("Twin", twinId);
			}
			EnsureTwinAccess(twin);

			BasicRelationship basicRelationship = null;

			try
			{
				basicRelationship = await _adtApiService.GetRelationship(InstanceSettings, twinId, id);
			}
			catch (AdtApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				return;
			}

			await _adtApiService.DeleteRelationship(InstanceSettings, twinId, id);

			try
			{
				await _adxHelper.AppendRelationship(this, basicRelationship, true);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error adding twin {Id} to ADX", basicRelationship.Id);
			}
		}

		public TwinWithRelationships[] FollowAllRelsToTargetModel(TwinWithRelationships twin, string[] relNames, string toModel)
		{
			throw new NotImplementedException();
		}

		#endregion

		public Task<AdtSiteStatsDto> GenerateADTInstanceStats()
		{
			throw new NotImplementedException();
		}

		#region ToRemove
		// TODO: remove method when cacheless is stable.
		public Task StartReloadFromAdtAsync()
		{
			throw new NotImplementedException();
		}

        #endregion

        /// <summary>
        /// Retrieves a collection of Digital Twins representing sites within a specified scope.
        /// If the provided dtId corresponds to a site, the returned list will contain that single site.
        /// If the provided dtId corresponds to a campus, portfolio, or any other entity that includes
        /// sites, the returned list will include all the sites directly or indirectly associated with
        /// the specified entity. Non-site entities, such as campuses, will not be included. For example,
        /// if the input is a portfolio that contains campuses, which in turn contain sites, only the
        /// sites will be included in the result, excluding the campuses.
        /// </summary>
        public async Task<IEnumerable<Twin>> GetSitesByScopeAsync(string twinId)
        {
            var sites = new ConcurrentBag<BasicDigitalTwin>();
            var initialTwin = await _adtApiService.GetTwin(InstanceSettings, twinId);

            await TraverseAndCollectSites(initialTwin, sites, GetDefaultModels(), new ConcurrentBag<string>());

            return Twin.MapFrom(sites);
        }

        /// <summary>
        /// Recursively traverses the digital twin hierarchy to collect all sites associated with
        /// the provided digital twin entity. If the input twin represents a site, it is added to
        /// the collection. If the input twin represents another entity, such as a building or a
        /// higher-level structure, the method explores its incoming relationships and includes
        /// all sites directly or indirectly associated with it. This process continues until
        /// all related sites within the hierarchy are collected.
        /// </summary>
        private async Task TraverseAndCollectSites(
            BasicDigitalTwin twin,
            ConcurrentBag<BasicDigitalTwin> sites,
            string[] modelIds,
            ConcurrentBag<string> seenTwinIds)
        {
            if (modelIds.Contains(twin.Metadata?.ModelId))
            {
                sites.Add(twin);
                seenTwinIds.Add(twin.Id);
            }
            else
            {
                var relationships = await _adtApiService.GetIncomingRelationshipsAsync(InstanceSettings, twin.Id);

                await Task.WhenAll(relationships.Select(async relationship =>
                {
                    if (!seenTwinIds.Contains(relationship.SourceId))
                    {
                        var sourceTwin = await _adtApiService.GetTwin(InstanceSettings, relationship.SourceId);
                        seenTwinIds.Add(sourceTwin.Id);
                        await TraverseAndCollectSites(sourceTwin, sites, modelIds, seenTwinIds);
                    }
                }));
            }
        }

        public async Task<IEnumerable<TwinMatchDto>> FindClosestWithCustomProperty(ClosestWithCustomPropertyQuery query)
        {
            if (!query.TwinIds.Any())
            {
                return Enumerable.Empty<TwinMatchDto>();
            }

            var customPropertyToFind = string.Join('.', query.CustomPropertyToFind.Split('.').Select(x => $"[[{x.TrimStart('[').TrimEnd(']')}]]"));

            var adtQueryImmediateMatches = AdtQueryBuilder.Create()
                .Select("DT.$dtId AS OriginalTwinId", "DT.$dtId AS ResolvedTwinId", $"DT.customProperties.{customPropertyToFind} AS ResolvedTwinCustomPropertyValue")
                .FromDigitalTwins("DT")
                .Where()
                .WithPropertyIn("DT.$dtId", query.TwinIds)
                .And()
                .IsDefined($"DT.customProperties.{customPropertyToFind}")
                .GetQuery();

            var adtQueryRelatedMatches = AdtQueryBuilder.Create()
                .Select("originalTwin.$dtId AS OriginalTwinId", "resolvedTwin.$dtId AS ResolvedTwinId", $"resolvedTwin.customProperties.{customPropertyToFind} AS ResolvedTwinCustomPropertyValue")
                .FromDigitalTwins()
                .Match(query.Relationships, "originalTwin", "resolvedTwin", $"*..{query.MaxNumberOfHops}")
                .Where()
                .WithPropertyIn("originalTwin.$dtId", query.TwinIds)
                .And()
                .CheckDefined(new List<string> { $"resolvedTwin.customProperties.{customPropertyToFind}" })
                .GetQuery();

            var immediateMatches = await _adtApiService.QueryTwins<TwinMatchDto>(InstanceSettings, adtQueryImmediateMatches).ToListAsync();
            var relatedMatches = await _adtApiService.QueryTwins<TwinMatchDto>(InstanceSettings, adtQueryRelatedMatches).ToListAsync();

            var result = immediateMatches.Union(relatedMatches);

            return query.TwinIds.Select(id => result.FirstOrDefault(x => x.OriginalTwinId == id) ?? new TwinMatchDto { OriginalTwinId = id });
        }

        // Below are copied from the ADT API /tree endpoint and will be removed once we switch to single-tenant.

        #region ADT API /tree endpoint copy

        /// <summary>
        /// Retrieve and process the full scope tree from ADT
        /// </summary>
        private async Task<IEnumerable<NestedTwin>> CalculateScopeTree()
        {
            var tree = await GetTreeAsync(
                CachelessAdtService.GetDefaultModels(),
                new string[] { "isPartOf", "isLocatedIn" },
                new string[] { },
                false
            );
            RemoveNonGuidSiteIds(tree);
            return tree;
        }

        /// <summary>
        /// We noticed that in CI, the non-leaf nodes have siteIDs which are not
        /// GUIDs; they match the dtIds. This causes PortalXL to crash when it reads
        /// the scopes tree because it tries to parse them as GUIDs. So this function
        /// traverses the tree and removes any siteIDs that are not GUIDs. Mutates
        /// `nodes`.
        /// </summary>
        private static void RemoveNonGuidSiteIds(IEnumerable<NestedTwin> nodes)
        {
            void recurse(NestedTwin node) {
                const string siteIdKey = "siteID";
                if (node.Twin.CustomProperties.ContainsKey(siteIdKey)) {
                    var siteId = node.Twin.CustomProperties[siteIdKey];
                    if (siteId is string)
                    {
                        Guid guid;
                        if (!Guid.TryParse(siteId as string, out guid)) {
                            node.Twin.CustomProperties.Remove(siteIdKey);
                        }
                    }
                }
                foreach (var child in node.Children)
                {
                    recurse(child);
                }
            };
            foreach (var s in nodes) {
                recurse(s);
            }
        }

        /// <summary>
        /// Sort the tree nodes by name. For our Walmart demo, we also have a special case
        /// where if two nodes are of the form "{some word} {some number}", we sort numerically
        /// by the number (so eg. "Region 9" comes before "Region 10").
        /// </summary>
        public static List<NestedTwin> SortTreeNodes(IEnumerable<NestedTwin> nodes)
        {
            var newList = new List<NestedTwin>(nodes);
            foreach (var node in newList)
            {
                if (node.Children != null)
                {
                    node.Children = SortTreeNodes(node.Children.ToList());
                }
            }
            newList.Sort((a, b) =>
            {
                var name1 = a.Twin.GetStringProperty("name");
                var name2 = b.Twin.GetStringProperty("name");
                if (name1 is not null && name2 is not null)
                {
                    var name1Parts = name1.Split(' ');
                    var name2Parts = name2.Split(' ');
                    if (name1Parts.Length == 2 && name2Parts.Length == 2 && name1Parts[0] == name2Parts[0])
                    {
                        int num1, num2;
                        if (int.TryParse(name1Parts[1], out num1) && int.TryParse(name2Parts[1], out num2))
                        {
                            return num1.CompareTo(num2);
                        }
                    }
                }

                return string.Compare(name1, name2);
            });
            return newList;
        }

        /// <summary>
        /// Retrieve the full scope tree from ADT and update the cache entry pointing to it
        /// </summary>
        public async Task UpdateScopeTree()
        {
            _memoryCache.Set(ScopeTreeCacheKey, await CalculateScopeTree());
        }

        /// <summary>
        /// Retrieve the latest scope tree from the cache. Because of the ScopeTreeUpdater service,
        /// the cache should always be hot, unless the service has just started and it hasn't finished
        /// populating yet.
        /// </summary>
        public async Task<IEnumerable<NestedTwin>> GetScopeTreeAsync()
        {
            return await _memoryCache.GetOrCreateAsync(ScopeTreeCacheKey, async (c) => await CalculateScopeTree());
        }

        /// <summary>
        /// Get twins and its children in tree form.
        /// Copied from the ADT API /tree endpoint and will be removed once we switch to single-tenant.
        /// </summary>
        /// <param name="models">Target model ids</param>
        /// <param name="outgoingRelationships">List of relationship types to be considered for traversal.
        /// <br/>             Default Values : ["isPartOf", "locatedIn"] will be used when relationshipsToTraverse is not supplied</param>
        /// <param name="incomingRelationships">List of relationship types to be considered for traversal.</param>
        /// <param name="exactModelMatch">Indicates if model filter must be exact match</param>
        public async Task<IEnumerable<NestedTwin>> GetTreeAsync(IEnumerable<string> models,
            IEnumerable<string> outgoingRelationships,
            IEnumerable<string> incomingRelationships,
            bool exactModelMatch = false)
        {
            var request = new GetTwinsInfoRequest
            {
                ModelId = (models == null || !models.Any()) ? GetDefaultModels() : models.ToArray(),
                ExactModelMatch = exactModelMatch
            };


            var page = await _adtApiService.GetTwinsAsync(InstanceSettings, request);
            var twins = await page.FetchAll(x => _adtApiService.GetTwinsAsync(InstanceSettings, request, continuationToken: x.ContinuationToken));

            if (!twins.Any())
                return Enumerable.Empty<NestedTwin>();

            var nestedTwins = twins.Select(x => new NestedTwin(Twin.MapFrom(x))).ToList();
            if (!outgoingRelationships.Any() && !incomingRelationships.Any())
                return nestedTwins;

            var processQueue = new List<NestedTwin>(nestedTwins);
            var seen = new ConcurrentBag<string>();

            // Dictionary's key is Nested twin's id, value is a tuple where item1 is parent's id and item2 is NestedTwin.
            var twinsMap = new ConcurrentDictionary<string, (string, NestedTwin)>();

            while (processQueue.Count > 0)
            {
                var newTwinsToProcess = new ConcurrentBag<NestedTwin>();
                await Parallel.ForEachAsync(processQueue, async (currentTwin, token) =>
                {
                    seen.Add(currentTwin.Twin.Id);

                    var parentIds = new ConcurrentBag<string>();

                    await FollowRelationshipsAsync(outgoingRelationships,
                        processQueue,
                        seen,
                        newTwinsToProcess,
                        x => x.TargetId,
                        () => _adtApiService.GetRelationshipsAsync(InstanceSettings, currentTwin.Twin.Id, outgoingRelationships.Count() == 1 ? outgoingRelationships.First() : null),
                        parentId => parentIds.Add(parentId));

                    await FollowRelationshipsAsync(incomingRelationships,
                        processQueue,
                        seen,
                        newTwinsToProcess,
                        x => x.SourceId,
                        () => _adtApiService.GetIncomingRelationshipsAsync(InstanceSettings, currentTwin.Twin.Id));

                    twinsMap.TryAdd(currentTwin.Twin.Id, (parentIds.Distinct().FirstOrDefault(), currentTwin));
                });

                processQueue.Clear();
                foreach (var twinToProcess in newTwinsToProcess)
                    processQueue.Add(twinToProcess);

                newTwinsToProcess.Clear();
            }

            SetParentsForChildren(twinsMap);

            return SortTreeNodes(GetTreeRoots(twinsMap));
        }

        public static string[] GetDefaultModels()
        {
           return
           [
               "dtmi:com:willowinc:Building;1",
               "dtmi:com:willowinc:Substructure;1",
               "dtmi:com:willowinc:OutdoorArea;1",
               "dtmi:com:willowinc:airport:AirportTerminal;1"
           ];
        }

        private async Task FollowRelationshipsAsync(IEnumerable<string> relationshipsToFollow,
            List<NestedTwin> processQueue,
            ConcurrentBag<string> seen,
            ConcurrentBag<NestedTwin> newTwinsToProcess,
            Func<BasicRelationship, string> getTwinId,
            Func<Task<IEnumerable<BasicRelationship>>> getRelationships,
            Action<string> processParent = null)
        {
            if (!relationshipsToFollow.Any())
                return;

            var relationships = await getRelationships();
            var enqueueTwins = relationships
                .Where(x => relationshipsToFollow.Contains(x.Name))
                .Select(async x =>
                {
                    var nextTwinId = getTwinId(x);
                    if (processQueue.All(q => q.Twin.Id != nextTwinId) && !seen.Contains(nextTwinId))
                    {
                        var twin = await _adtApiService.GetTwin(InstanceSettings, nextTwinId);
                        newTwinsToProcess.Add(new NestedTwin(Twin.MapFrom(twin)));
                    }

                    if (processParent != null)
                        processParent(nextTwinId);
                });

            await Task.WhenAll(enqueueTwins);
        }

        private static void SetParentsForChildren(ConcurrentDictionary<string, (string, NestedTwin)> twinsMap)
        {
            foreach (var twinInfo in twinsMap.Select(x => x.Value))
            {
                if (!string.IsNullOrEmpty(twinInfo.Item1) && twinsMap.ContainsKey(twinInfo.Item1))
                    twinsMap[twinInfo.Item1].Item2.Children.Add(twinInfo.Item2);
            }
        }

        private static IEnumerable<NestedTwin> GetTreeRoots(ConcurrentDictionary<string, (string, NestedTwin)> twinsMap)
        {
            return twinsMap.Where(x => string.IsNullOrEmpty(x.Value.Item1)).Select(x => x.Value.Item2);
        }

        #endregion
    }
}
