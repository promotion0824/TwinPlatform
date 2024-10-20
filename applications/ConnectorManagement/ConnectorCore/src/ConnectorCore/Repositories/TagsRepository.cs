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
    using Dapper;

    internal class TagsRepository : ITagsRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public TagsRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task<IList<TagEntity>> GetAllAsync()
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = "select * from [dbo].[Tag]";
                var data = await conn.QueryAsync<TagEntity>(sql);
                return data.ToList();
            }
        }

        public async Task<IList<TagEntity>> GetByPointIdAsync(Guid pointId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select t.*
                            from [dbo].[PointTag] pt, [dbo].[Tag] t
                            where pt.[TagId] = t.[Id] and pt.[PointEntityId] = @pointId";
                var data = await conn.QueryAsync<TagEntity>(sql, new { pointId });
                return data.ToList();
            }
        }

        public async Task<Dictionary<Guid, List<TagEntity>>> GetByPointIdsAsync(IEnumerable<Guid> pointIds)
        {
            if (!pointIds.Any())
            {
                return new Dictionary<Guid, List<TagEntity>>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var idsString = string.Join(",", pointIds.Select(q => $"'{q}'"));

                var sql = $@"select pt.PointEntityId, t.*
                        from [dbo].[Tag] t, [PointTag] pt
                        where pt.[TagId] = t.[Id] and pt.[PointEntityId] in ({idsString})";

                var data = await conn.QueryAsync<Guid, TagEntity, (TagEntity Tag, Guid PointEntityId)>(sql,
                    (pointEntityId, tag) => (tag, pointEntityId));

                var result = data.GroupBy(q => q.PointEntityId).ToDictionary(q => q.Key, q => q.Select(qq => qq.Tag).ToList());
                return result;
            }
        }

        public async Task<Dictionary<Guid, List<TagEntity>>> GetEquipmentTagsBySiteIdAsync(Guid siteId)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = $@"select e.Id as [EquipmentId], t.*
                            from [Equipment] e,  [EquipmentTag] et, [Tag] t
                            where et.[EquipmentId] = e.[Id] and et.[TagId] = t.[Id] and e.[SiteId] = @siteId";

                var data = await conn.QueryAsync<Guid, TagEntity, TagEquipmentPair>(sql,
                    (equipmentId, tag) => new TagEquipmentPair
                    {
                        EquipmentId = equipmentId,
                        Tag = tag,
                    },
                    new
                    {
                        siteId,
                    });

                return data.GroupBy(item => item.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Select(item => item.Tag).ToList());
            }
        }

        public async Task<Dictionary<Guid, List<TagEntity>>> GetEquipmentTagsByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds)
        {
            if (!equipmentIds.Any())
            {
                return new Dictionary<Guid, List<TagEntity>>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = $@"with EquipmentIds as
                        (select cast(value as uniqueidentifier) as [EquipmentId] from string_split('{string.Join(",", equipmentIds)}', ','))
                        select e.EquipmentId, t.*
                        from [dbo].[Tag] t, [EquipmentIds] e
                        where exists(select 1 from [dbo].[EquipmentTag] et where et.[TagId] = t.[Id] and et.[EquipmentId] = e.[EquipmentId])";

                var data = await conn.QueryAsync<Guid, TagEntity, TagEquipmentPair>(sql,
                    (equipmentId, tag) => new TagEquipmentPair { EquipmentId = equipmentId, Tag = tag });

                return data.GroupBy(item => item.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Select(item => item.Tag).ToList());
            }
        }

        public async Task<Dictionary<Guid, List<TagEntity>>> GetPointTagsByEquipmentIdsAsync(IEnumerable<Guid> equipmentIds)
        {
            if (!equipmentIds.Any())
            {
                return new Dictionary<Guid, List<TagEntity>>();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = $@"with EquipmentIds as
                        (select cast(value as uniqueidentifier) as [EquipmentId] from string_split('{string.Join(",", equipmentIds)}', ','))
                        select e.EquipmentId, t.*
                        from [dbo].[Tag] t, [EquipmentIds] e
                        where exists(select 1 from [EquipmentPoint] ep, [PointTag] pt where ep.[EquipmentId] = e.[EquipmentId] and pt.[PointEntityId] = ep.[PointEntityId] and pt.[TagId] = t.[Id])";

                var data = await conn.QueryAsync<Guid, TagEntity, TagEquipmentPair>(sql,
                    (equipmentId, tag) => new TagEquipmentPair { EquipmentId = equipmentId, Tag = tag });

                return data.GroupBy(item => item.EquipmentId)
                    .ToDictionary(g => g.Key, g => g.Select(item => item.Tag).ToList());
            }
        }

        public async Task<TagEntity> CreateAsync(TagEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var selectSql = "select t.* from [dbo].[Tag] t where t.[Name] = @Name and t.[ClientId] = @ClientId";
                var existingTag = await conn.QuerySingleOrDefaultAsync<TagEntity>(selectSql, newItem);
                if (existingTag != null)
                {
                    throw new DuplicateNameException($"[{typeof(TagEntity).Name}] with provided Name and ClientId already exists: {newItem.Name} {newItem.ClientId}");
                }

                var sql = "insert into [dbo].[Tag]([Id], [Name], [Description], [ClientId], [Feature]) values(@Id, @Name, @Description, @ClientId, @Feature)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        private class TagEquipmentPair
        {
            public Guid EquipmentId { get; set; }

            public TagEntity Tag { get; set; }
        }
    }
}
