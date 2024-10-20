using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Willow.RealEstate.Command.Generated;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace WillowRules.Test.Bugs.Mocks
{
	public class CommandInsightServiceMock : ICommandInsightService
	{
		public int LastOccurrenceCount = 0;
		public bool WasCalled = false;

		public Task<HttpStatusCode> CloseInsightInCommand(Insight insight)
		{
			return Task.FromResult(HttpStatusCode.OK);
		}

		public Task<HttpStatusCode> DeleteInsightFromCommand(Insight insight)
		{
			return Task.FromResult(HttpStatusCode.OK);
		}

		public Task<IEnumerable<InsightSimpleDto>> GetInsightsForSiteId(Guid siteId)
		{
			throw new NotImplementedException();
		}

		public Task TryAcquireToken()
		{
			throw new NotImplementedException();
		}

		public Task<HttpStatusCode> UpsertInsightToCommand(Insight insight)
		{
			LastOccurrenceCount = insight.Occurrences.Count;
			insight.InsightSynced(insight.Status, insight.CommandInsightId);
			WasCalled = true;
			return Task.FromResult(HttpStatusCode.OK);
		}
	}
}
