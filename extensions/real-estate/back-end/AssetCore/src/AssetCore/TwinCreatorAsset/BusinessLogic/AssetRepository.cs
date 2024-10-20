using AutoMapper;
using AssetCoreTwinCreator.Constants.Schema;
using AssetCoreTwinCreator.Domain;
using AssetCoreTwinCreator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetCoreTwinCreator.MappingId;
using AssetCoreTwinCreator.Services;
using Newtonsoft.Json;
using Willow.Infrastructure.Exceptions;
using AssetCore.TwinCreatorAsset.Models;

namespace AssetCoreTwinCreator.BusinessLogic
{
	public interface IAssetRepository
	{
		Task<Dictionary<int, IEnumerable<AssetParameter>>> GetAssetCategoryData(List<int> assetRegisterIds, Domain.Models.Category categoryWithColumns, IDbContextTransaction transaction = null);
		Task<Asset> Get(int assetRegisterId);
		Task<IEnumerable<ChangeHistoryRecord>> GetChangeHistory(int assetId);
		Task<List<Asset>> GetAssetsAsync(int buildingId, int[] categoryIds = null, string floorCode = null, string searchKeyword = null, bool includeProperties = false);
		Task<List<Asset>> GetBuildingAssets(int buildingId, int[] categoryIds);

		Task<Asset> GetByAssetIdentifier(int buildingId, string assetIdentifier, bool includeProperties);
		Task<Asset> GetByForgeViewerModelId(int buildingId, string forgeViewerModelId, bool includeProperties);
		Task<List<AssetName>> GetAssetsNamesAsync(IEnumerable<int> assetsIds);
	}

	public class AssetRepository : IAssetRepository
	{
		private readonly AssetDbContext _dbContext;
		private readonly IMapper _mapper;
		private ILogger<AssetRepository> _logger;
		private readonly MappingDbContext _mapContext;
		private readonly IAssetRegisterIndexCacheService _cacheService;

		public AssetRepository(AssetDbContext dbContext, IMapper mapper, ILogger<AssetRepository> logger, MappingDbContext mapContext, IAssetRegisterIndexCacheService cacheService)
		{
			_dbContext = dbContext;
			_mapper = mapper;
			_logger = logger;
			_mapContext = mapContext;
			_cacheService = cacheService;
		}

		public async Task<List<Asset>> GetAssetsAsync(int buildingId, int[] categoryIds = null, string floorCode = null, string searchKeyword = null, bool includeProperties = false)
		{
			var cache = await _cacheService.GetCacheAsync(buildingId);
			var cacheItems = cache.SearchAssets(
				categoryIds: (categoryIds == null) ? null : categoryIds,
				floorCodes: floorCode == null ? null : new[] { floorCode },
				searchKeywords: string.IsNullOrWhiteSpace(searchKeyword) ? null : searchKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				);

			if (!cacheItems.Any())
			{
				return new List<Asset>();
			}

			var cacheIds = cacheItems.Select(a => a.Id).ToList();

			var entities = new List<Domain.Models.Asset>();

			const int batchSize = 20000;
			for (int batch = 0; batch * batchSize < cacheIds.Count; batch++)
			{
				var ids = cacheIds.Skip(batch * batchSize).Take(batchSize).ToList();
				var entitiesQuery = _dbContext.Assets
				.AsNoTracking()
				.Where(a => ids.Contains(a.Id));

				entities.AddRange(await entitiesQuery.ToListAsync());
			}

			var entityCategoryIds = entities.Select(e => e.CategoryId).Distinct();
			var categories = (await _dbContext.Categories.Include(c => c.CategoryColumns).Where(c => entityCategoryIds.Contains(c.Id)).ToListAsync()).ToDictionary(c => c.Id);

			foreach (var entity in entities)
			{
				entity.Category = categories[entity.CategoryId];
			}

			var assets = entities.Select(e => _mapper.Map<Asset>(e)).ToList();

			if (includeProperties)
			{
				if (assets.Any())
				{
					await IncludeProperties(assets, entities.First().Category);
				}
			}

			await IncludeGeometry(assets);
			return assets;
		}

		public async Task<List<Asset>> GetBuildingAssets(int buildingId, int[] categoryIds)
		{
			var entities = await _dbContext.Assets
				.Where(a => a.BuildingId == buildingId && !a.Archived && categoryIds.Contains(a.CategoryId))
				.Select(a => new Domain.Models.Asset
				{
					Id = a.Id,
					FloorCode = a.FloorCode,
					Identifier = a.Identifier,
					CategoryId = a.CategoryId,
					Name = a.Name,
					ForgeViewerModelId = a.ForgeViewerModelId
				})
				.ToListAsync();

			var entityCategoryIds = entities.Select(e => e.CategoryId).Distinct();
			var categories = (await _dbContext.Categories.Include(c => c.CategoryColumns).Where(c => entityCategoryIds.Contains(c.Id)).ToListAsync()).ToDictionary(c => c.Id);

			foreach (var entity in entities)
			{
				entity.Category = categories[entity.CategoryId];
				entity.BuildingId = buildingId;
			}

			var assets = entities.Select(e => _mapper.Map<Asset>(e)).ToList();

			await IncludeGeometry(assets);
			return assets;
		}

		public async Task<Asset> Get(int assetRegisterId)
		{
			var entity = await _dbContext.Assets
				.Include(a => a.Category)
				.ThenInclude(c => c.CategoryColumns)
				.AsNoTracking()
				.FirstOrDefaultAsync(a => a.Id == assetRegisterId);

			if (entity == null)
			{
				return null;
			}

			var result = _mapper.Map<Asset>(entity);

			await IncludeProperties(result, entity.Category);
			await IncludeGeometry(result);

			return result;
		}

		private Task IncludeProperties(Asset asset, Domain.Models.Category category)
		{
			return IncludeProperties(new Asset[] { asset }, category);
		}

		private async Task IncludeProperties(IEnumerable<Asset> assets, Domain.Models.Category category)
		{
			var parameters = await GetAssetCategoryData(assets.Select(a => a.Id).ToList(), category);
			if (parameters.Any())
			{
				foreach (var asset in assets)
				{
					if (parameters.ContainsKey(asset.Id))
					{
						asset.AssetParameters = parameters[asset.Id];
					}
				}
			}
		}

		private async Task IncludeGeometry(IEnumerable<Asset> assets)
		{
			var assetMap = assets.ToDictionary(x => x.Id);
			var allGeometries = await _mapContext.AssetGeometries.ToListAsync();
			foreach (var geometry in allGeometries)
			{
				if (assetMap.TryGetValue(geometry.AssetRegisterId, out Asset asset))
				{
					asset.Geometry = JsonConvert.DeserializeObject<List<double>>(geometry.Geometry);
				}
			}
		}

		private async Task IncludeGeometry(Asset asset)
		{
			await IncludeGeometry(new[] {asset});
		}

		public async Task<Asset> GetByAssetIdentifier(int buildingId, string assetIdentifier, bool includeProperties)
		{
			try
			{
				var entity = await _dbContext.Assets
					.Include(a => a.Category)
					.ThenInclude(c => c.CategoryColumns)
					.AsNoTracking()
					.SingleOrDefaultAsync(a =>
						a.BuildingId == buildingId && a.Identifier == assetIdentifier);

				if (entity == null)
				{
					return null;
				}

				var result = _mapper.Map<Asset>(entity);

				if (includeProperties)
				{
					await IncludeProperties(result, entity.Category);
					await IncludeGeometry(result);
				}

				return result;
			}
			catch (InvalidOperationException e)
			{
				throw new ResourceNotFoundException(nameof(Asset), assetIdentifier, $"Multiple assets are found by identifier {assetIdentifier}", e);
			}
		}

		/// <returns>Key = asset register ID, value = asset parameters from dynamic category table</returns>
		public virtual async Task<Dictionary<int, IEnumerable<AssetParameter>>> GetAssetCategoryData(List<int> assetRegisterIds, Domain.Models.Category categoryWithColumns, IDbContextTransaction transaction = null)
		{
			return await DoGetAssetCategoryData(assetRegisterIds, categoryWithColumns, transaction);
		}

		private async Task<Dictionary<int, IEnumerable<AssetParameter>>> DoGetAssetCategoryData(List<int> assetRegisterIds, Domain.Models.Category categoryWithColumns, IDbContextTransaction transaction = null)
		{
			// TODO: update this method to use a table valued parameter and use a join to improve performance
			const int batchSize = 2000; // SQL parameter limit of 2100
			var result = new Dictionary<int, IEnumerable<AssetParameter>>();

			if (assetRegisterIds.Any())
			{
				try
				{
					for (int batch = 0; batch * batchSize < assetRegisterIds.Count; batch++)
					{
						var arIds = assetRegisterIds.Skip(batch * batchSize).Take(batchSize).ToList();

						using (var command = _dbContext.Database.GetDbConnection().CreateCommand())
						{
							var sql = new StringBuilder($"SELECT * FROM [{categoryWithColumns.DbTableName}] WHERE AssetRegisterId IN ({string.Join(",", arIds)})");

							command.Transaction = transaction?.GetDbTransaction();
							command.CommandText = sql.ToString();

							await _dbContext.Database.OpenConnectionAsync();
							var reader = await command.ExecuteReaderAsync();

							while (await reader.ReadAsync())
							{
								int? assetRegisterId = null;
								var assetParams = new List<AssetParameter>();

								for (var i = 0; i < reader.FieldCount; i++)
								{
									var colName = reader.GetName(i);
									var value = reader.GetValue(i);
									var catCol = categoryWithColumns.CategoryColumns.FirstOrDefault(cc => cc.DbColumnName == colName);

									if (value is DBNull)
									{
										value = null;
									}

									if (colName == ReservedNames.ColumnNameAssetRegisterId)
									{
										assetRegisterId = Convert.ToInt32(value);
									}

									if (catCol == null)
									{
										continue;
									}

									assetParams.Add(new AssetParameter
									{
										Key = colName,
										DisplayName = catCol.Name,
										Value = value
									});
								}

								result[assetRegisterId.Value] = assetParams;
							}

							reader.Close();
						}
					}
				}
				catch (Exception exception)
				{
					_logger.LogError($"Function 'DoGetAssetCategoryData' failed in AssetCore. exception.ToString(): [[{exception.ToString()} ]]");
				}
			}
			return result;
		}

		public async Task<IEnumerable<ChangeHistoryRecord>> GetChangeHistory(int assetId)
		{
			var changeHistory = await _dbContext.AssetChangeLogs
			.AsNoTracking()
			.Where(x => x.AssetRegisterId == assetId)
			.Select(x => new
			{
				Change = x.ColumnDisplayName,
				OldValue = x.ValueOld,
				NewValue = x.ValueNew,
				Date = x.ChangedOn,
				UserId = x.ChangedBy
			})
			.ToListAsync();

			//var userIds = changeHistory.Select(x => x.UserId).Distinct();

			//var users = await _identityDbContext.Users
			//.AsNoTracking()
			//.Where(x => userIds.Contains(x.Id))
			//.ToDictionaryAsync(x => x.Id, x => string.IsNullOrEmpty(x.NameFirst) ? x.Email : $"{x.NameFirst} {x.NameLast}");
			var users = new Dictionary<Guid, string>();

			return changeHistory.Select(x => new ChangeHistoryRecord
			{
				Change = x.Change,
				OldValue = x.OldValue,
				NewValue = x.NewValue,
				Date = x.Date,
				Name = users.ContainsKey(x.UserId) ? users[x.UserId] : "Unknown"
			});
		}

		public async Task<Asset> GetByForgeViewerModelId(int buildingId, string forgeViewerModelId, bool includeProperties)
		{
			var entity = await _dbContext.Assets
				.Include(a => a.Category)
				.ThenInclude(c => c.CategoryColumns)
				.AsNoTracking()
				.Where(a => a.BuildingId == buildingId && !a.Archived && !a.Category.Archived && a.ForgeViewerModelId == forgeViewerModelId)
				.FirstOrDefaultAsync();

			if (entity == null)
			{
				return null;
			}

			var result = _mapper.Map<Asset>(entity);

			if (includeProperties)
			{
				await IncludeProperties(result, entity.Category);
				await IncludeGeometry(result);
			}

			return result;
		}
		
		public async Task<List<AssetName>> GetAssetsNamesAsync(IEnumerable<int> assetsIds)
		{
			if(!assetsIds?.Any() ?? true)
			{
				return new List<AssetName>();
			}

			var assetsNames = await _dbContext.Assets
										  .Where(x => assetsIds.Contains(x.Id)).Select(x => new AssetName { AssetRegisterId = x.Id, Name = x.Name })
										  .ToListAsync();

			return assetsNames;
		}
	}	
}