using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Configuration;
using Willow.Rules.Configuration.Customer;
using Willow.Rules.Model;
using Willow.Rules.Repository;
using Willow.Rules.Services;

namespace WillowRules.Test.Processors
{
	[TestClass]
	public class DataQualityServiceTest
	{
		/// <summary>
		/// Only used to test local integration. Ignored by default
		/// </summary>
		[TestMethod, Ignore]
		public async Task MustIntegrateToADTApi()
		{
			var dataQualityService = new DataQualityService(
				Mock.Of<IADTApiService>(),
				Mock.Of<ILogger<DataQualityService>>());

			var backgroundService = new DataQualityBackgroundService(
				dataQualityService,
				Mock.Of<ILogger<DataQualityBackgroundService>>());

			var timeSeries = new TimeSeries(Guid.NewGuid().ToString(), "");
			timeSeries.DtId = "random-test-id";
			timeSeries.ExternalId = Guid.NewGuid().ToString();
			timeSeries.ConnectorId = Guid.NewGuid().ToString();

			var buffers = new[] { timeSeries };

			var service = backgroundService.StartAsync(CancellationToken.None);
			await dataQualityService.SendCapabilityStatusUpdate(buffers);
			//await Task.Delay(10000);
		}
	}
}
