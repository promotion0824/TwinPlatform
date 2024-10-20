using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AssetCoreTwinCreator.BusinessLogic;
using AssetCoreTwinCreator.Models;

namespace AssetCoreTwinCreator.Features.Asset.Search
{
    public interface ICategoryStructure
    {
        Task<List<CategoryColumn>> GetStructureForCategory(int categoryId);
    }

    public class CategoryStructure : ICategoryStructure
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryStructure(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<List<CategoryColumn>> GetStructureForCategory(int categoryId)
        {
            var category = await _categoryRepository.GetCategoryWithColumns(categoryId);

            var mappedCategoryColumns = category == null ? new CategoryColumn[0] : _mapper.Map<IEnumerable<CategoryColumn>>(category.CategoryColumns);

            return mappedCategoryColumns.ToList();
        }
    }
}
