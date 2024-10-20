using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Repository;

namespace WillowRules.Test.Bugs.Mocks;

public class RepositoryTimeSeriesMappingMock : RepositoryBaseMock<TimeSeriesMapping>, IRepositoryTimeSeriesMapping
{
	public Task<int> DeleteBefore(DateTimeOffset date, CancellationToken cancellationToken = default)
	{
		throw new NotImplementedException();
	}
}

