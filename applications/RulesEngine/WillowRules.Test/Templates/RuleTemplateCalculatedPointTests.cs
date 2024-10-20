using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.Rules.Configuration;
using Microsoft.Extensions.Logging;
using WillowRules.Services;
using Azure.Identity;
using System.Threading;

namespace WillowRules.Test.Templates;

[TestClass]
public class RuleTemplateCalculatedPointTests
{
	[TestMethod, Ignore]
	public async Task WriteToADX()
	{
		var logger = new Mock<ILogger<EventHubService>>().Object;
		var listenerLogger = new Mock<ILogger<EventHubBackgroundService>>().Object;

		var customerOptions = Options.Create<CustomerOptions>(new CustomerOptions()
		{
			EventHub = new EventHubSettings()
			{
				NamespaceName = $"wil-uat-lda-cu1-aue1-uie",
				//NamespaceName = $"wil-dev-lda-cu1-aue1-uie",
				QueueName = "ingestion-to-adx"
			}
		});

		var credential =
			new DefaultAzureCredential(
				new DefaultAzureCredentialOptions
				{
					ExcludeVisualStudioCodeCredential = true,
					ExcludeVisualStudioCredential = true,
					ExcludeSharedTokenCacheCredential = false,
					ExcludeAzurePowerShellCredential = true,
					ExcludeEnvironmentCredential = true,
					ExcludeInteractiveBrowserCredential = true
				});

		var eventHubService = new EventHubService(logger);
		var listenerService = new EventHubBackgroundService(customerOptions, eventHubService,
			new RulesEngine.Processor.Services.HealthCheckCalculatedPoints(),
			credential, listenerLogger);

		await eventHubService.Writer.WriteAsync(new EventHubServiceDto()
		{
			ConnectorId = "CalculatedPoint",
			TrendId = Guid.NewGuid().ToString(),
			SourceTimestamp = DateTime.UtcNow.AddHours(-1),
			EnqueuedTimestamp = DateTime.UtcNow,
			ScalarValue = 15
		});

		Assert.AreEqual(1, eventHubService.Reader.Count);

		await listenerService.StartAsync(CancellationToken.None);

		Assert.AreEqual(0, eventHubService.Reader.Count);

		await listenerService.StopAsync(CancellationToken.None);
	}
}
