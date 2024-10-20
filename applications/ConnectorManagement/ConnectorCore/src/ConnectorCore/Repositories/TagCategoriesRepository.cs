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

    internal class TagCategoriesRepository : ITagCategoriesRepository
    {
        private readonly IDbConnectionProvider connectionProvider;

        public TagCategoriesRepository(IDbConnectionProvider connectionProvider)
        {
            this.connectionProvider = connectionProvider;
        }

        public async Task AddTagsToCategoryAsync(Guid categoryId, IEnumerable<Guid> tagIds)
        {
            tagIds = tagIds.ToList();

            if (!tagIds.Any())
            {
                return;
            }

            var inputSql = string.Join("\nunion\n", tagIds.Select(tagId => $"select '{tagId}' as [TagId], '{categoryId}' as [CategoryId]"));

            var sql = $@"with input as ({inputSql})
                        insert into [dbo].[TagCategory]([TagId], [CategoryId])
                        select i.[TagId], i.[CategoryId] from input i
                        where not exists(select 1 from [dbo].[TagCategory] tg where tg.[TagId] = i.[TagId] and tg.[CategoryId] = i.[CategoryId])";

            using (var conn = await connectionProvider.GetConnection())
            {
                await conn.ExecuteAsync(sql);
            }
        }

        public async Task<IList<CategoryEntity>> GetTagCategoriesAsync(bool? withTags = null)
        {
            using (var conn = await connectionProvider.GetConnection())
            {
                var sql = @"select c.*,
                                   cast((case when exists(select 1 from [dbo].[Category] c2 where c2.ParentId = c.Id) then 1 else 0 end) as bit) as [HasChildren]
                            from [dbo].[Category] c";
                var data = await conn.QueryAsync<CategoryEntity>(sql);
                var categories = data.ToList();

                if ((withTags ?? false) && categories.Any())
                {
                    var tagsSql = @"select tg.*, t.*
                            from [TagCategory] tg, [Tag] t
                            where tg.[TagId] = t.[Id]";

                    var pairsList = new List<TagLinkPair>();
                    var result = await conn.QueryAsync<TagCategoryLinkEntity, TagEntity, TagCategoryLinkEntity>(tagsSql,
                        (link, tag) =>
                        {
                            pairsList.Add(new TagLinkPair { Link = link, Tag = tag });
                            return link;
                        });

                    var tagsDict = pairsList.GroupBy(p => p.Link.CategoryId).ToDictionary(g => g.Key, g => g.Select(p => p.Tag));
                    foreach (var tagCategoryEntity in categories)
                    {
                        if (tagsDict.TryGetValue(tagCategoryEntity.Id, out var tags))
                        {
                            tagCategoryEntity.Tags = tags.ToList();
                        }
                    }
                }

                return categories;
            }
        }

        public async Task<CategoryEntity> CreateTagCategoryAsync(CategoryEntity newItem)
        {
            if (newItem.Id == Guid.Empty)
            {
                newItem.Id = Guid.NewGuid();
            }

            using (var conn = await connectionProvider.GetConnection())
            {
                var selectSql = "select t.* from [dbo].[Tag] t where t.[Name] = @Name and t.[ClientId] = @ClientId";
                var existingCategory = await conn.QuerySingleOrDefaultAsync<CategoryEntity>(selectSql, newItem);
                if (existingCategory != null)
                {
                    throw new DuplicateNameException($"[{typeof(CategoryEntity).Name}] with provided Name and ClientId already exists: {newItem.Name} {newItem.ClientId}");
                }

                var sql = "insert into [dbo].[Category]([Id], [Name], [ParentId], [ClientId]) values(@Id, @Name, @ParentId, @ClientId)";
                await conn.ExecuteAsync(sql, newItem);
            }

            return newItem;
        }

        private class TagLinkPair
        {
            public TagCategoryLinkEntity Link { get; set; }

            public TagEntity Tag { get; set; }
        }
    }
}
