using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace RulesEngine.Benchmarks
{
	public class Program
	{
		static void Main(string[] args)
		{
			BenchmarkRunner.Run<ParserBenchmarks>();
			BenchmarkRunner.Run<TemplateBenchmarks>();
			BenchmarkRunner.Run<EnvTests>();
		}
	}
}