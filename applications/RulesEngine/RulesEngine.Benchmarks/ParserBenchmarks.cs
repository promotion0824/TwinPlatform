using BenchmarkDotNet.Attributes;
using Willow.ExpressionParser;

namespace RulesEngine.Benchmarks
{
	[JsonExporterAttribute.Full]
	public class ParserBenchmarks
	{
		[Benchmark]
		public void CanParseNumbers()
		{
			Parser.Deserialize("1");
			Parser.Deserialize("-200");
			Parser.Deserialize("-202.2");
		}

		[Benchmark]
		public void CanParsePercentages()
		{
			Parser.Deserialize("10%");
			Parser.Deserialize("110%");
		}

		[Benchmark]
		public void CanParseVariableNames()
		{
			Parser.Deserialize("A");
			Parser.Deserialize("Aa1234_");
			Parser.Deserialize("$_abcd");
		}

		[Benchmark]
		public void CanParseVariableNamesWithSquareParenthesesA()
		{
			Parser.Deserialize("[A]");
		}

		[Benchmark]
		public void CanParseVariableNamesWithSquareParenthesesSpace()
		{
			Parser.Deserialize("[foo bar]");
		}

		[Benchmark]
		public void CanParseVariableNamesWithSquareParenthesesBadChars()
		{
			Parser.Deserialize("[A;1]");
		}

		[Benchmark]
		public void CanParseStrings()
		{
			Parser.Deserialize("\"A\"");
			Parser.Deserialize("'B'");
		}


		[Benchmark]
		public void CanParseFunctions()
		{
			Parser.Deserialize("A(5)");
			Parser.Deserialize("foo(2, 3)");
			Parser.Deserialize("bar(0, 'str', 5.4)");
		}

		[Benchmark]
		public void CanParseFailedFunction()
		{
			Parser.Deserialize("OPTION(FAILED([dtmi:com:willowinc:AirHumiditySetpoint;1]))");
		}

		[Benchmark]
		public void CanParseNumericExpressions()
		{
			Parser.Deserialize("1 + 2 + 3 + B");
			Parser.Deserialize("1 + 5 * 6 / A - 2 * 3");
			Parser.Deserialize("1 + 2 - 3 * 4 / 5 ^ 6");
		}

		[Benchmark]
		public void CanParseComparisonExpressions()
		{
			Parser.Deserialize("A > 23");
			Parser.Deserialize("B <= 27");
			Parser.Deserialize("'abc' >= 0.5");
		}

		[Benchmark]
		public void CanParseLogicalExpressions()
		{
			Parser.Deserialize("A & B OR ! C");
			Parser.Deserialize("! A OR ! B & C OR D");
			Parser.Deserialize("a < 5 AND b >= 6");
		}

		[Benchmark]
		public void RealExample_CheckPrcedence()
		{
			string expr = "([air_flow_sp_ratio] > 1.1) & [damper_cmd] < 0.05";
			Parser.Deserialize(expr);
		}
	}
}
