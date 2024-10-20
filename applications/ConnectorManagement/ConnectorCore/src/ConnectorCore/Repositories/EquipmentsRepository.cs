namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading.Tasks;
    using ConnectorCore.Database;
    using ConnectorCore.Entities;
    using ConnectorCore.Models;
    using Dapper;

    internal class EquipmentsRepository : IEquipmentsRepository
    {
        private static readonly string SqlSelect = $@"SELECT e.[Id], e.[Name], e.[ClientId], e.[SiteId], e.[FloorId], e.[ExternalEquipmentId], e.[Category], e.[ParentId] FROM [dbo].[Equipment] AS e";
        private readonly IDbConnectionProvider connectionProvider;

        public EquipmentsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<List<EquipmentEntity>> GetBySiteIdAsync(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = SqlSelect + @" WHERE e.[SiteId] = @siteId ORDER BY e.[Id]";
                var data = await conn.QueryAsync<EquipmentEntity>(sql, new { siteId });
                return data.ToList();
            }
        }

        public async Task<List<EquipmentEntity>> GetByIdsAsync(IEnumerable<Guid> equipmentIds)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = SqlSelect + @" WHERE e.[Id] IN @equipmentIds";
                var data = await conn.QueryAsync<EquipmentEntity>(sql, new { equipmentIds });
                return data.ToList();
            }
        }

        public async Task<IList<Guid>> GetAllIdsForConnectorIdAsync(Guid connectorId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select distinct ep.EquipmentId from [EquipmentPoint] ep
                                  inner join [Point] p on ep.PointEntityId = p.EntityId
                                  inner join [Device] d on p.DeviceId = d.Id
                                where d.[ConnectorId] = @connectorId";
                var data = await conn.QueryAsync<Guid>(sql, new { connectorId });
                return data.ToList();
            }
        }

        public async Task<List<EquipmentEntity>> GetByPointIdAsync(Guid pointEntityId)
        {
            var items = await GetByPointIdsAsync(new[] { pointEntityId });
            if (items.TryGetValue(pointEntityId, out var result))
            {
                return result;
            }

            return new List<EquipmentEntity>();
        }

        public async Task<Dictionary<Guid, List<EquipmentEntity>>> GetByPointIdsAsync(IEnumerable<Guid> pointEntityIds)
        {
            pointEntityIds = pointEntityIds?.ToList();
            if (pointEntityIds == null || !pointEntityIds.Any())
            {
                return new Dictionary<Guid, List<EquipmentEntity>>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var pointsParameter = string.Join(",", pointEntityIds.Select(p => $"'{p}'"));
                var sql = $@"WITH equipments AS ({SqlSelect})
                                SELECT ep.*, equipments.* FROM equipments, [dbo].[EquipmentPoint] ep
                                WHERE ep.[EquipmentId] = equipments.[Id] AND ep.PointEntityId IN ({pointsParameter})";

                var data = await conn.QueryAsync<EquipmentToPointLink, EquipmentEntity, PointLinkEquipmentPair>(sql,
                    (epl, equipment) => new PointLinkEquipmentPair { PointLink = epl, Equipment = equipment });

                return data.GroupBy(item => item.PointLink.PointEntityId)
                    .ToDictionary(g => g.Key, g => g.Select(item => item.Equipment).ToList());
            }
        }

        public async Task AddTagsToEquipmentAsync(List<EquipmentToTagLink> links)
        {
            await AddOrReplaceTagsToEquipmentAsync(links.Select(l => l.EquipmentId).ToList(), links, false);
        }

        private async Task AddOrReplaceTagsToEquipmentAsync(List<Guid> equipments, List<EquipmentToTagLink> links, bool deletedOld)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                using (var ts = conn.BeginTransaction())
                {
                    var dataTableEquipmentTag = new DataTable("EquipmentTagType");
                    dataTableEquipmentTag.Columns.Add("EquipmentId", typeof(Guid));
                    dataTableEquipmentTag.Columns.Add("TagId", typeof(Guid));
                    foreach (var record in links)
                    {
                        dataTableEquipmentTag.Rows.Add(record.EquipmentId, record.TagId);
                    }

                    try
                    {
                        if (deletedOld && equipments.Any())
                        {
                            var sqlDelete = $"DELETE FROM [dbo].[EquipmentTag] WHERE EquipmentId IN ({string.Join(",", equipments.Select(id => $"'{id:D}'"))})";
                            await conn.ExecuteAsync(sqlDelete, transaction: ts);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DeletedRowInaccessibleException($"[{typeof(EquipmentToTagLink).Name}] can't be deleted: " + e.Message, e);
                    }

                    var sql = "insert into [dbo].[EquipmentTag] select * from @ut";
                    await conn.ExecuteAsync(sql,
                        new { @ut = dataTableEquipmentTag.AsTableValuedParameter("[dbo].[EquipmentTagType]") },
                        ts);

                    ts.Commit();
                }
            }
        }

        public async Task AddPointsToEquipmentAsync(List<EquipmentToPointLink> links)
        {
            await AddOrReplacePointsToEquipmentAsync(links.Select(l => l.EquipmentId).ToList(), links, false);
        }

        private async Task AddOrReplacePointsToEquipmentAsync(List<Guid> equipments, List<EquipmentToPointLink> links, bool deletedOld)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                using (var ts = conn.BeginTransaction())
                {
                    var dataTableEquipmentTag = new DataTable("EquipmentPointType");
                    dataTableEquipmentTag.Columns.Add("EquipmentId", typeof(Guid));
                    dataTableEquipmentTag.Columns.Add("PointEntityId", typeof(Guid));
                    foreach (var record in links)
                    {
                        dataTableEquipmentTag.Rows.Add(record.EquipmentId, record.PointEntityId);
                    }

                    try
                    {
                        if (deletedOld && equipments.Any())
                        {
                            var sqlDelete = $"delete from [dbo].[EquipmentPoint] where EquipmentId in ({string.Join(",", equipments.Select(id => $"'{id:D}'"))})";
                            await conn.ExecuteAsync(sqlDelete, transaction: ts);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new DeletedRowInaccessibleException($"[{typeof(EquipmentToPointLink).Name}] can't be deleted: " + e.Message, e);
                    }

                    var sql = "insert into [dbo].[EquipmentPoint] select * from @ut";
                    await conn.ExecuteAsync(sql,
                        new { @ut = dataTableEquipmentTag.AsTableValuedParameter("[dbo].[EquipmentPointType]") },
                        ts);

                    ts.Commit();
                }
            }
        }

        public async Task<EquipmentEntity> CreateAsync(EquipmentEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                const string sql = @"INSERT INTO [dbo].[Equipment] ([Id], [Name], [ClientId], [SiteId], [FloorId], [ExternalEquipmentId], [ParentId], [Category])
                            VALUES (@Id, @Name, @ClientId, @SiteId, @FloorId, @ExternalEquipmentId, @ParentId, @Category);";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        private class PointLinkEquipmentPair
        {
            public EquipmentToPointLink PointLink { get; set; }

            public EquipmentEntity Equipment { get; set; }
        }
    }
}
