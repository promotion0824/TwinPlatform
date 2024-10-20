using BenchmarkDotNet.Attributes;
using Willow.Expressions;

namespace RulesEngine.Benchmarks
{
	[JsonExporterAttribute.Full]
	public class EnvTests
	{
		[Benchmark]
		public void GetAndSetVariables()
		{
			var env = Env.Empty.Push();

			for (var i = 0; i < 10; i++)
			{
				env.Assign(i.ToString(), i);
			}

			for (var i = 0; i < 1000; i++)
			{
				for (var x = 0; x < 10; x++)
				{
					env.Get(x.ToString());
				}
			}
		}
	}
}
