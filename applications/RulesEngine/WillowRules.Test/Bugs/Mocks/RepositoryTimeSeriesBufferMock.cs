using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryTimeSeriesBufferMock : RepositoryBaseMock<TimeSeries>, IRepositoryTimeSeriesBuffer
{
	public Task<TimeSeries?> GetByTwinId(string twinId)
	{
		throw new System.NotImplementedException();
	}

	public Task RemoveAll()
	{
		throw new System.NotImplementedException();
	}
}
