using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CachelessMigrationTests
{
	public abstract class BaseTest : IRunnable
	{
		private string TestName { get { return $"{this.GetType().Name}{(AdxImplemented ? " [ADX]" : string.Empty)}"; } }
		protected abstract string UrlFormat { get; }
		protected virtual ICollection<KeyValuePair<string, string>> QueryParam { get { return null;  } }
		protected virtual bool AdxImplemented { get { return false; } }

		public void Run(string[] args)
		{
			var timeoutSeconds = 100;
			var timeoutIndex = Array.IndexOf(args, "--timeout");
			if (timeoutIndex > -1)
				int.TryParse(args[timeoutIndex + 1].Trim(), out timeoutSeconds);

			var caller = new Caller(timeoutSeconds);
			var localRes = GetCachlessDtCoreResult(caller);

			if (args.Contains("--compare"))
			{
				var uatRes = GetCurrentDtCoreResult(caller);

				TestOutput.Process(TestName, uatRes, localRes, (x,y) => CompareResponseContent(x,y));
			}
			else
			{
				TestOutput.Process(TestName, localRes);
			}
		}

		protected virtual void CompareResponseContent(Result oldRes, Result newRes)
		{
			if (oldRes.Content.StartsWith("["))
			{
				var (oldCount, newCount, oldExtra, newExtra) = GetItemCount(oldRes, newRes);
				Console.WriteLine($"Response Number of items: old {oldCount} vs new {newCount}.");
				if (oldExtra.Any())
					Console.WriteLine($"Ids not returned by new: {string.Join(',', oldExtra)}");
				if (newExtra.Any())
					Console.WriteLine($"Extra ids returned by new: {string.Join(',', newExtra)}");
			}
		}

		private (int, int, IEnumerable<string>, IEnumerable<string>) GetItemCount(Result oldRes, Result newRes)
		{
			var oldItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(oldRes.Content);
			var newItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(newRes.Content);

			var oldIds = oldItems.Select(i => i["id"] as string).ToList();
			var newIds = newItems.Select(i => i["id"] as string).ToList();

			var oldExtra = oldIds.Where(o => !newIds.Any(n => n == o));
			var newExtra = newIds.Where(n => !oldIds.Any(o => o == n));

			return (oldItems.Count, newItems.Count, oldExtra, newExtra);
		}

		protected virtual Result GetCachlessDtCoreResult(Caller caller)
		{
			var localTask = caller.Get(string.Format(UrlFormat, Urls.LocalUrl), QueryParam);
			return localTask.GetAwaiter().GetResult();
		}

		protected virtual Result GetCurrentDtCoreResult(Caller caller)
		{
			var uatTask = caller.Get(string.Format(UrlFormat, Urls.UatUrl), QueryParam);
			return uatTask.GetAwaiter().GetResult();
		}
	}
}
