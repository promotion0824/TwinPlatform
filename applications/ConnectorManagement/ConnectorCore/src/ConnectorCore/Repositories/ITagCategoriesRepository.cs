namespace ConnectorCore.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ConnectorCore.Entities;

    internal interface ITagCategoriesRepository
    {
        Task<IList<CategoryEntity>> GetTagCategoriesAsync(bool? withTags = null);

        Task<CategoryEntity> CreateTagCategoryAsync(CategoryEntity newItem);

        Task AddTagsToCategoryAsync(Guid categoryId, IEnumerable<Guid> tagIds);
    }
}
