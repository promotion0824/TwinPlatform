namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using Dapper;

    internal class PointsRepository : IPointsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public PointsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<List<PointEntity>> GetByTagNameAsync(Guid siteId, string tagName)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select p.* from [dbo].[Point] p
                            where p.[SiteId] = @siteId
                            and exists (select 1 from [dbo].[PointTag] pt, [dbo].[Tag] t where pt.[TagId] = t.[Id] and pt.[PointEntityId] =p.[EntityId] and t.[Name] = @tagName)";
                var data = await conn.QueryAsync<PointEntity>(sql, new { siteId, tagName });
                return data.ToList();
            }
        }

        public async Task<IList<PointEntity>> GetBySiteIdDeviceIdAsync(Guid siteId, Guid deviceId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Point] where [DeviceId] = @deviceId and [SiteId] = @siteId";
                var data = await conn.QueryAsync<PointEntity>(sql, new { deviceId, siteId });
                return data.ToList();
            }
        }

        public async Task<IList<PointEntity>> GetBySiteIdEquipmentIdAsync(Guid siteId, Guid equipmentId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select p.* " +
                    "from [dbo].[Point] p inner join [dbo].[EquipmentPoint] ep on p.[EntityId] = ep.[PointEntityId] " +
                    "where ep.[EquipmentId] = @equipmentId and p.[SiteId] = @siteId";

                var data = await conn.QueryAsync<PointEntity>(sql, new { equipmentId, siteId });
                return data.ToList();
            }
        }

        public async Task<Dictionary<Guid, List<PointEntity>>> GetByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds)
        {
            if (!equipmentIds.Any())
            {
                return new Dictionary<Guid, List<PointEntity>>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select ep.*, p.*
                            from [dbo].[Point] p, [dbo].[EquipmentPoint] ep
                            where ep.[PointEntityId] = p.[EntityId] and ep.[EquipmentId] in @equipmentIds";
                var data = await conn.QueryAsync<EquipmentToPointLink, PointEntity, PointLinkEquipmentPair>(sql,
                    (link, point) => new PointLinkEquipmentPair { PointLink = link, Point = point },
                    new { equipmentIds });

                return data.GroupBy(item => item.PointLink.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Select(item => item.Point).ToList());
            }
        }

        public async Task<IList<int>> GetAllPointTypesAsync()
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select Id from [dbo].[PointType]";

                var data = await conn.QueryAsync<int>(sql);
                return data.ToList();
            }
        }

        public async Task<IList<string>> GetAllExternalPointsForSiteExcludingConnectorAsync(Guid siteId, Guid connectorId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select distinct p.ExternalPointId from [dbo].[Point] p
                            inner join [dbo].[Device] d on p.[DeviceId] = d.[Id]
                          where p.[SiteId] = @siteId and d.[ConnectorId] <> @connectorId";

                var data = await conn.QueryAsync<string>(sql, new { siteId, connectorId });
                return data.ToList();
            }
        }

        public async Task<Dictionary<Guid, Guid>> GetEntityIdByPointIdMappingAsync(Guid connectorId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select p.Id, p.EntityId from [dbo].[Point] p
                            inner join [dbo].[Device] d on p.DeviceId = d.Id
                            where d.ConnectorId =  @connectorId";
                var data = await conn.QueryAsync<PointEntity>(sql, new { connectorId });
                return data.ToDictionary(x => x.Id, x => x.EntityId);
            }
        }

        public async Task<PointIdentifier> GetPointIdentifierByExternalPointIdAsync(Guid siteId, string externalPointId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql =
                    "select ep.[EquipmentId], p.[EntityId] as PointEntityId from [dbo].[Point] p left join [dbo].[EquipmentPoint] ep on ep.[PointEntityId] = p.[EntityId] where [SiteId] = @siteId and [ExternalPointId] = @externalPointId";
                return await conn.QuerySingleOrDefaultAsync<PointIdentifier>(sql, new { siteId, externalPointId });
            }
        }

        public async Task<IList<PointEntity>> GetBySiteIdAsync(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Point] where [SiteId] = @siteId";
                var data = await conn.QueryAsync<PointEntity>(sql, new { siteId });
                return data.ToList();
            }
        }

        public async Task<IList<PointEntity>> GetByConnectorIdAsync(Guid connectorId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select p.*,d.* from [dbo].[Point] p left join [dbo].[Device] d on p.[DeviceId]=d.[Id]
                            where d.[ConnectorId] = @connectorId";
                var data = await conn.QueryAsync<PointEntity, DeviceEntity, PointEntity>(sql,
                    (p, d) =>
                    {
                        p.Device = d;
                        return p;
                    },
                    new
                    {
                        connectorId,
                    });
                return data.ToList();
            }
        }

        public async Task AddTagsToPointAsync(List<PointToTagLink> links)
        {
            await AddOrReplaceTagsToPointAsync(links.Select(l => l.PointId).ToList(), links, false);
        }

        private async Task AddOrReplaceTagsToPointAsync(List<Guid> points, List<PointToTagLink> links, bool deletedOld)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                using (var ts = conn.BeginTransaction())
                {
                    var dataTablePointTag = new DataTable("PointTagType");
                    dataTablePointTag.Columns.Add("PointEntityId", typeof(Guid));
                    dataTablePointTag.Columns.Add("TagId", typeof(Guid));
                    foreach (var record in links)
                    {
                        dataTablePointTag.Rows.Add(record.PointId, record.TagId);
                    }

                    try
                    {
                        if (deletedOld && points.Any())
                        {
                            var sqlDelete = $"delete from [dbo].[PointTag] where [PointEntityId] in ({string.Join(",", points.Select(id => $"'{id:D}'"))})";
                            await conn.ExecuteAsync(sqlDelete, transaction: ts);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DeletedRowInaccessibleException($"[{typeof(PointToTagLink).Name}] can't be deleted: " + e.Message, e);
                    }

                    var sqlInsert = "insert into [dbo].[PointTag] select * from @ut";
                    await conn.ExecuteAsync(sqlInsert,
                        new
                        {
                            @ut = dataTablePointTag.AsTableValuedParameter("[dbo].[PointTagType]"),
                        },
                        ts);

                    ts.Commit();
                }
            }
        }

        public async Task<PointEntity> GetPointById(Guid pointId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Point] where [Id] = @pointId";
                var data = await conn.QuerySingleOrDefaultAsync<PointEntity>(sql, new { pointId });
                return data;
            }
        }

        public async Task<PointEntity> GetPointByEntityId(Guid pointEntityId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Point] where [EntityId] = @pointEntityId";
                var data = await conn.QuerySingleOrDefaultAsync<PointEntity>(sql, new { pointEntityId });
                return data;
            }
        }

        public async Task<IList<PointEntity>> GetByEntityIds(Guid[] entityIds)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = "select * from [dbo].[Point] where [EntityId] in @entityIds";
                var data = await conn.QueryAsync<PointEntity>(sql, new { entityIds });
                return data.ToList();
            }
        }

        public async Task<PointEntity> GetPointByExternalId(Guid deviceId, string externalPointId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select p.*
                            from [dbo].[Point] p, [dbo].[Device] d
                            where
                                p.[DeviceId] = d.[Id]
                            and p.[ExternalPointId] = @externalPointId
                            and d.[ConnectorId] = (select d2.ConnectorId from [dbo].[Device] d2 where d2.Id = @deviceId)";
                var data = await conn.QuerySingleOrDefaultAsync<PointEntity>(sql, new { deviceId, externalPointId });
                return data;
            }
        }

        public async Task<PointEntity> CreateAsync(PointEntity newItem)
        {
            if (!string.IsNullOrEmpty(newItem.ExternalPointId))
            {
                //if the point with the same external id already exists for connector - ignore the insert
                var existingPoint = await GetPointByExternalId(newItem.DeviceId, newItem.ExternalPointId);
                if (existingPoint != null)
                {
                    return existingPoint;
                }
            }

            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO [dbo].[Point] ([Id], [EntityId], [Name], [ClientId], [SiteId], [Unit], [Type], [ExternalPointId],
		                            [Category], [Metadata], [IsDetected], [DeviceId], [IsEnabled])
                            VALUES (@Id, @EntityId, @Name, @ClientId, @SiteId, @Unit, @Type, @ExternalPointId,
					                @Category, @Metadata, @IsDetected, @DeviceId, @IsEnabled);";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        private class PointLinkEquipmentPair
        {
            public EquipmentToPointLink PointLink { get; set; }

            public PointEntity Point { get; set; }
        }
    }
}
