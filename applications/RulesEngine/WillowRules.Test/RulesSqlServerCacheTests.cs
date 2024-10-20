using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.Rules.Cache;

namespace WillowRules.Test;

[TestClass]
public class RulesSqlServerCacheTests
{
	public static IConfiguration InitConfiguration()
	{
		// Need to have an appsettings.test.json file for these tests to work
		string configFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, "appsettings.test.json");
		if (!System.IO.File.Exists(configFilePath)) return new ConfigurationBuilder().Build(); ;

		var config = new ConfigurationBuilder()
		   .AddJsonFile("appsettings.test.json")
			.AddEnvironmentVariables()
			.Build();
		return config;
	}

	private RulesSqlServerCache? cache;
	private string connectionString = string.Empty;

	[TestInitialize]
	public void TestSetup()
	{
		var config = InitConfiguration();

		connectionString = config["ConnectionString"]!;

		if (!string.IsNullOrEmpty(connectionString))
		{
			cache = new RulesSqlServerCache(connectionString, Mock.Of<ILogger<RulesSqlServerCache>>());
		}
	}

	[TestMethod]
	public async Task MustSetGetAndRemoveCache()
	{
		if (cache is null)
		{
			Assert.Inconclusive("Cache Tests disabled due to now settings");
			return;
		}

		using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
		{
			var key = "mykey";
			var value = new byte[] { 1 };

			await cache.SetAsync(key, value, TimeSpan.FromHours(1));

			var cachedValue = await cache.GetAsync(key);

			cachedValue.Should().HaveCount(1);
			cachedValue![0].Should().Be(1);

			var keys = await cache.GetAllKeysAsync("my").ToListAsync();

			keys.Should().HaveCount(1);
			keys[0].Should().Be(key);

			var allValues = await cache.GetAllValuesAsync("my").ToListAsync();

			allValues.Should().HaveCount(1);
			allValues[0][0].Should().Be(1);

			await cache.RemoveAsync(key);

			cachedValue = await cache.GetAsync(key);

			cachedValue.Should().BeNull();
		}
	}

	[TestMethod]
	public async Task MustRemoveByDate()
	{
		if (cache is null)
		{
			Assert.Inconclusive("Cache Tests disabled due to now settings");
			return;
		}

		using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
		{
			var key = "mykey";
			var value = new byte[] { 1 };

			await cache.SetAsync(key, value, TimeSpan.FromHours(1));

			int deleteCount = await cache.RemoveAsync("my", DateTimeOffset.UtcNow.AddSeconds(3));

			var exitingValue = await cache.GetAllValuesAsync(key).FirstOrDefaultAsync();

			deleteCount.Should().Be(1);

			exitingValue.Should().BeNull();
		}
	}

	[TestMethod]
	public async Task ShouldNotReturnIfExpired()
	{
		if (cache is null)
		{
			Assert.Inconclusive("Cache Tests disabled due to no settings");
			return;
		}

		using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
		{
			var key = "mykey";
			var value = new byte[] { 1 };

			await cache.SetAsync(key, value, TimeSpan.FromMilliseconds(250));

			await Task.Delay(500);

			var cachedValue = await cache.GetAsync(key);

			cachedValue.Should().BeNull();
		}
	}

	[TestMethod]
	public async Task MustExecuteExpiryCache()
	{
		if (cache is null)
		{
			Assert.Inconclusive("Cache Tests disabled due to no settings");
			return;
		}

		cache = new RulesSqlServerCache(connectionString, Mock.Of<ILogger<RulesSqlServerCache>>(), TimeSpan.FromMilliseconds(300));

		using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, TransactionScopeAsyncFlowOption.Enabled))
		{
			var key = "mykey";
			var value = new byte[] { 1 };

			await cache.SetAsync(key, value, TimeSpan.FromMilliseconds(150));

			await Task.Delay(5000);

			await cache.GetAsync(key);

			var keys = await cache.GetAllKeysAsync("my").ToListAsync();

			keys.Should().BeEmpty();
		}
	}
}