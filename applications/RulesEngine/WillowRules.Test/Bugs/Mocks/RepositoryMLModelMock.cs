using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Willow.Rules.DTO;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryMLModelMock : RepositoryBaseMock<MLModel>, IRepositoryMLModel
{
	public Task<Batch<MLModel>> GetAllModels(SortSpecificationDto[] sortSpecifications, FilterSpecificationDto[] filterSpecifications, Expression<Func<MLModel, bool>>? whereExpression = null, int? page = null, int? take = null)
	{
		throw new NotImplementedException();
	}

	public IEnumerable<MLModel> GetAllModels()
	{
		return Data;
	}

	public MLModel? GetModel(string id)
	{
		return Data.FirstOrDefault(v => v.Id == id);
	}

	public MLModel? GetModelByFullName(string fullName)
	{
		return Data.FirstOrDefault(v => v.FullName == fullName);
	}

	public Task<IEnumerable<(string id, string name)>> GetModelNames()
	{
		return Task.FromResult(Data.Select(v => (v.Id, v.FullName)));
	}

	public IAsyncEnumerable<MLModel> GetModelsWithoutBinary()
	{
		return Data.ToAsyncEnumerable();
	}

	public MLModel? GetModelWithoutBinary(string id)
	{
		return Data.FirstOrDefault(v => v.Id == id);
	}
}
