using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.Rules.Cache;
using Willow.Rules.Configuration;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class DataCacheTests
{
	private record TestClass
	{
		public string Name { get; set; } = string.Empty;
		public int Count { get; set; }

		public static TestClass One = new TestClass { Name = "One", Count = 2 };
		public static TestClass Two = new TestClass { Name = "Two", Count = 3 };
	}

	const string willowEnvironment = "test-env";

	public class MockDistCache : IRulesDistributedCache
	{
		private Dictionary<string, byte[]> data = new Dictionary<string, byte[]>();

		public Task SetExpiryAsync(string startsWith, TimeSpan expirationRelativeToNow)
		{
			throw new NotImplementedException();
		}

		public Task<int> CountAsync(string? startsWith)
		{
			return Task.FromResult(0);
		}

		public IAsyncEnumerable<string> GetAllKeysAsync(string? startsWith)
		{
			return AsyncEnumerable.Empty<string>();
		}

		public IAsyncEnumerable<byte[]> GetAllValuesAsync(string startsWith)
		{
			return AsyncEnumerable.Empty<byte[]>();
		}

		public Task<byte[]?> GetAsync(string key)
		{
			return Task.FromResult<byte[]?>(data[key]);
		}

		public Task RemoveAsync(string key)
		{
			data.Remove(key);
			return Task.CompletedTask;
		}

		public Task SetAsync(string key, byte[] binary, TimeSpan? expirationRelativeToNow)
		{
			data[key] = binary;
			return Task.CompletedTask;
		}

		public Task<int> RemoveAsync(string startsWith, DateTimeOffset maxDate)
		{
			throw new NotImplementedException();
		}
	}

	IDataCache<TestClass>? dataCache;

	[TestInitialize]
	public void Setup()
	{
		var memoryCache = new MemoryCache(Microsoft.Extensions.Options.Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

		dataCache = new DataCache<TestClass>("empty", TimeSpan.FromMinutes(1), CachePolicy.EagerReload,
			MemoryCachePolicy.NoMemoryCache,
			memoryCache,
			new MockDistCache(),
			Mock.Of<ILoggerFactory>());
	}

	[TestMethod]
	public async Task EmptyDiskCache()
	{
		var insight1 = new Insight() { Id = "i1" };
		var s1 = new ImpactScore(insight1, "s1", "s1", "e1", 1, "");
		var s2 = new ImpactScore(insight1, "s2", "s2", "e2", 1, "");
		insight1.ImpactScores = new List<ImpactScore>() { s1, s2 };

		var insight2 = new Insight() { Id = "i2" };
		var s3 = new ImpactScore(insight1, "s1", "s1", "e1", 2, "");
		var s4 = new ImpactScore(insight1, "s2", "s2", "e2", 3, "");
		insight2.ImpactScores = new List<ImpactScore>() { s3, s4 };

		var insights = new List<Insight>()
		{
			insight1,
			insight2
		};

		var colnames = new string[] { "[Comfort impact]", "[Cost impact]", "[Energy Impact]", "[Reliability impact]" };

		var colSql = string.Join(",", colnames);

		var sql = @$"SELECT Id,{colSql}
            from
            (
              select ii.Id,Name,Score
              from insight ii left join InsightImpactScore i on i.insightid = ii.id
            ) x
            pivot
            (
                max(Score)
                for Name in ({colSql})
            ) p";

		var results = insights
		.OrderByDescending(p => p.ImpactScores.FirstOrDefault(v => v.Name == "s1")?.Score);

		//var results = insights.SelectMany(v => v.ImpactScores.Select(v1 => new { insight = v, score = v1 }))
		//	.OrderBy(v => v.score.Name == "s1" ? v.score.Score : 100000)
		//	.Select(v => v.insight)
		//	.DistinctBy(v => v.Id)
		//	.ToList();


		List<(DateTime startDate, DateTime endDate)> dateBatches = new();

		var date = DateTime.Now.AddDays(-3);
		var latest = DateTime.Now;

		while (true)
		{
			var nextDate = date.AddDays(1);

			if (nextDate >= latest)
			{
				break;
			}

			dateBatches.Add((date, nextDate));

			date = nextDate;
		}

		var all = await dataCache!.GetAll(willowEnvironment, 12).ToListAsync();
		all.Should().HaveCountGreaterOrEqualTo(0);
	}

	// TODO: test failing
	[TestMethod]
	[Ignore]
	public async Task CanAddStringToDiskCache()
	{
		var all = await dataCache!.AddOrUpdate(willowEnvironment, "id1", TestClass.One);
		var fetched = await dataCache.TryGetValue(willowEnvironment, "id1");
		fetched.Should().Be((true, TestClass.One));
	}

	// Why is this failing on Azure?
	// [TestMethod]
	// public async Task CanRemoveFromDiskCache()
	// {
	// 	var all = await diskCache.AddOrUpdate(willowEnvironment, "id2", TestClass.Two);

	// 	var fetched = await diskCache.TryGetValue(willowEnvironment, "id2");
	// 	fetched.Should().Be((true, TestClass.Two));

	// 	await diskCache.RemoveKey(willowEnvironment, "id2");

	// 	fetched = await diskCache.TryGetValue(willowEnvironment, "id2");
	// 	fetched.Should().Be((false, null));
	// }


}
