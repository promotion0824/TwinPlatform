using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace CachelessMigrationTests
{
	public static class TestOutput
	{
		private static List<TestResult> TestsResults = new List<TestResult>();

		public static void Process(string testName, Result oldRes, Result newRes, Action<Result, Result> compareContent = null)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(testName);
			Console.ResetColor();
			
			var oldTimeSpan = Math.Round(oldRes.Span.TotalSeconds, 2);
			var newTimeSpan = Math.Round(newRes.Span.TotalSeconds, 2);
			
			Console.WriteLine($"Response: {(newRes.Success ? "Success" : $"Failed ({newRes.Error})")}");
			Console.WriteLine($"Performance: old - {oldTimeSpan}s, new - {newTimeSpan}s");
			Console.WriteLine($"Response Content Size: old - {Math.Round(Convert.ToDouble(oldRes.ContentLenght.Value / 1024), 2)}KB, new - {Math.Round(Convert.ToDouble(newRes.ContentLenght.Value / 1024), 2)}KB");
			if (compareContent != null)
				compareContent(oldRes, newRes);
			Console.WriteLine();

			var testResult = new TestResult { Name = testName, CachelessDtCoreResult = newRes, CurrentDtCoreResult = oldRes, MatchingContent = oldRes.Content == newRes.Content };

			//Simplifying output
			testResult.CachelessDtCoreResult.Content = null;
			testResult.CurrentDtCoreResult.Content = null;

			TestsResults.Add(testResult);
		}		

		public static void Process(string testName, Result result)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(testName);
			Console.ResetColor();

			var newTimeSpan = Math.Round(result.Span.TotalSeconds, 2);

			Console.WriteLine($"Response: {(result.Success ? "Success" : $"Failed ({result.Error})")}");
			Console.WriteLine($"Performance: {newTimeSpan}s");
			if(result.Success)
				Console.WriteLine($"Response Content Size: {Math.Round(Convert.ToDouble(result.ContentLenght.Value / 1024), 2)}KB");
			Console.WriteLine();

			TestsResults.Add(new TestResult { Name = testName, CachelessDtCoreResult = result });
		}

		public static void GenerateOutput(string[] args)
		{
			var index = Array.IndexOf(args, "--output");
			if (index > -1)
			{
				var outputDir = args[index + 1];
				
				//Simplifying output
				var resultString = System.Text.Json.JsonSerializer.Serialize(new
				{
					TestResults = TestsResults.Select(x => GetSummaryResult(x, args)),
					Queries = GetExecutedQueries(args)
						.Select(x => new { Path = x.Key, Queries = x.Value.GroupBy(q => q).Select(r => new { Query = r.Key, ExecutionCount = r.Count()})})
				});

				var outputFile = Path.Combine(outputDir, $"testresult.{DateTime.Now:MM.dd.yy.H.mm.ss}.json");

				File.WriteAllText(outputFile, resultString);

				Console.WriteLine();
				Console.WriteLine($"Output file located in {outputFile}");
			}
		}

		private static Dictionary<string, List<string>> GetExecutedQueries(string[] args)
		{
			if (!args.Contains("--includequeries"))
			{
				return new Dictionary<string, List<string>>();
			}

			var localTask = new Caller().Get($"{Urls.LocalUrl}/admin/sites/{UatData.SiteId1MW}/twins/queries");
			var result = localTask.GetAwaiter().GetResult();
			return JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(result.Content);
		}

		private static object GetSummaryResult(TestResult testResult, string[] args)
		{
			if (args.Contains("--compare"))
			{
				return new
				{
					TestName = testResult.Name,
					Summary = new
					{
						CachelessDtCorePath = testResult.CachelessDtCoreResult.Url,
						CurrentDtCorePath = testResult.CurrentDtCoreResult.Url,
						Performance = $"Current {Math.Round(testResult.CurrentDtCoreResult.Span.TotalSeconds, 2)}s vs New {Math.Round(testResult.CachelessDtCoreResult.Span.TotalSeconds, 2)}s",
						ResponsePayload = $"{(!testResult.MatchingContent ? "Different" : "Matching")} content.",
						RequestResult = $"Current DtCore {(testResult.CurrentDtCoreResult.Success ? "succeeded" : $"failed (error: {testResult.CurrentDtCoreResult.Error})")} / Cacheless DtCore {(testResult.CachelessDtCoreResult.Success ? "succeeded" : $"failed (error: {testResult.CachelessDtCoreResult.Error})")}"
					}
				};
			}

			return new
			{
				TestName = testResult.Name,
				Summary = new
				{
					Path = testResult.CachelessDtCoreResult.Url,
					Performance = $"{Math.Round(testResult.CachelessDtCoreResult.Span.TotalSeconds, 2)}s",
					RequestResult = $"{(testResult.CachelessDtCoreResult.Success ? "Succeeded" : $"Failed (error: {testResult.CachelessDtCoreResult.Error})")}",
					ResponseSize = $"{Math.Round(Convert.ToDouble(testResult.CachelessDtCoreResult.ContentLenght.Value / 1024), 2)}KB"
				}
			};
		}
	}
}
