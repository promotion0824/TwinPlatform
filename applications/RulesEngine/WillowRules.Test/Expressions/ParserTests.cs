using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.ExpressionParser;
using Willow.Expressions;
using System.Linq;
using System;
using System.IO;
using Willow.Expressions.Visitor;

namespace WillowExpressions.Test;

#pragma warning disable CS8602 // Dereference of a possibly null reference.

[TestClass]
public class ParserTests
{
	private static void CheckType<T>(TokenExpression expression)
	{
		expression.Should().BeOfType<T>();
	}

	private static void CheckSerialized(TokenExpression expression, string str)
	{
		expression.Serialize().Should().Be(str);
	}

	[TestMethod]
	public void CanParseDegCCharacter()
	{
		TokenExpression expr1 = Parser.Deserialize("5°C");
		CheckSerialized(expr1, "5[degC]");
	}

	[TestMethod]
	public void CanParseNumbers()
	{
		TokenExpression expr1 = Parser.Deserialize("1");
		ParserTests.CheckSerialized(expr1, "1");

		TokenExpression expr2 = Parser.Deserialize("-200");
		ParserTests.CheckSerialized(expr2, "-200");

		TokenExpression expr3 = Parser.Deserialize("-202.2");
		ParserTests.CheckSerialized(expr3, "-202.2");
	}

	[TestMethod]
	public void CanParseUnidentifiedUnit()
	{
		TokenExpression expr1 = Parser.Deserialize("(1occ) + 1occ + 5");
		ParserTests.CheckSerialized(expr1, "1[occ] + 1[occ] + 5");
	}

	[TestMethod]
	public void CanParsePercentages()
	{
		TokenExpression expr1 = Parser.Deserialize("10%");
		ParserTests.CheckSerialized(expr1, "10[%]");

		TokenExpression expr2 = Parser.Deserialize("110%");
		ParserTests.CheckSerialized(expr2, "110[%]");
	}

	[DataRow("A")]
	[DataRow("Aa1234_")]
	[DataRow("_abcd")]
	[DataRow("$_abcd")]
	[DataRow("débit")]
	[DataRow("débit")]
	[DataRow("hôpital")]
	[DataRow("ancêtre")]
	[DataRow("tâche")]
	[DataRow("août")]
	//[DataRow("AXA_STO_CPT_EF_B_1_SANI_B21_Débitd'eau")] this fails because of the accent
	[TestMethod]
	public void CanParseVariableNames(string input)
	{
		TokenExpression expr1 = Parser.Deserialize(input);
		ParserTests.CheckType<TokenExpressionVariableAccess>(expr1);
		((TokenExpressionVariableAccess)expr1).VariableName.Should().Be(input);
		ParserTests.CheckSerialized(expr1, input);
	}

	[DataRow("A")]
	[DataRow("Aa1234_")]
	[DataRow("_abcd")]
	[TestMethod]
	public void CanParseVariableNamesWithSquareParenthesesA(string input)
	{
		TokenExpression expr1 = Parser.Deserialize($"[{input}]");
		expr1.Should().BeOfType<TokenExpressionVariableAccess>();
		expr1.ToString().Should().Be(input);
		ParserTests.CheckSerialized(expr1, input);
	}

	[DataRow("AXA_STO_CPT_EF_B_1_SANI_B21_Débitd'eau")]
	[TestMethod]
	public void CanParseVariableNamesWithSquareParenthesesAndTheyKeepSquares(string input)
	{
		TokenExpression expr1 = Parser.Deserialize($"[{input}]");
		expr1.Should().BeOfType<TokenExpressionVariableAccess>();
		expr1.ToString().Should().Be(input);
		ParserTests.CheckSerialized(expr1, $"[{input}]");
	}

	[TestMethod]
	public void CanParseVariableNamesWithSquareParenthesesSpace()
	{
		TokenExpression expr2 = Parser.Deserialize("[foo bar]");
		expr2.Should().BeOfType<TokenExpressionVariableAccess>();
		expr2.ToString().Should().Be("foo bar");
		ParserTests.CheckSerialized(expr2, "[foo bar]");
	}

	[TestMethod]
	public void CanParseVariableNamesWithSquareParenthesesBadChars()
	{
		TokenExpression expr3 = Parser.Deserialize("[A;1]");
		expr3.Should().BeOfType<TokenExpressionVariableAccess>();
		expr3.ToString().Should().Be("A;1");
		ParserTests.CheckSerialized(expr3, "[A;1]");
	}

	[TestMethod]
	public void CanParseStrings()
	{
		TokenExpression expr1 = Parser.Deserialize("\"A\"");
		expr1.Should().BeOfType<TokenExpressionConstantString>();
		((TokenExpressionConstantString)expr1).ValueString.Should().Be("A");
		ParserTests.CheckSerialized(expr1, "\"A\"");

		TokenExpression expr2 = Parser.Deserialize("'B'");
		expr2.Should().BeOfType<TokenExpressionConstantString>();
		((TokenExpressionConstantString)expr2).ValueString.Should().Be("B");
		ParserTests.CheckSerialized(expr2, "\"B\"");
	}


	[TestMethod]
	public void CanParseFunctions()
	{
		TokenExpression expr1 = Parser.Deserialize("A(5)");
		ParserTests.CheckType<TokenExpressionFunctionCall>(expr1);
		TokenExpressionFunctionCall func1 = ((TokenExpressionFunctionCall)expr1);
		func1.FunctionName.Should().Be("A");
		func1.Children.Length.Should().Be(1);
		ParserTests.CheckSerialized(expr1, "A(5)");

		TokenExpression expr2 = Parser.Deserialize("foo(2, 3)");
		ParserTests.CheckType<TokenExpressionFunctionCall>(expr2);
		TokenExpressionFunctionCall func2 = ((TokenExpressionFunctionCall)expr2);
		func2.FunctionName.Should().Be("foo");
		func2.Children.Length.Should().Be(2);
		ParserTests.CheckSerialized(expr2, "foo(2,3)");

		TokenExpression expr3 = Parser.Deserialize("bar(0, 'str', 5.4)");
		ParserTests.CheckType<TokenExpressionFunctionCall>(expr3);
		TokenExpressionFunctionCall func3 = ((TokenExpressionFunctionCall)expr3);
		func3.FunctionName.Should().Be("bar");
		func3.Children.Length.Should().Be(3);
		ParserTests.CheckSerialized(expr3, "bar(0,\"str\",5.4)");
	}

	[TestMethod]
	public void CanParseFailedFunction()
	{
		TokenExpression expr1 = Parser.Deserialize("OPTION(FAILED([dtmi:com:willowinc:AirHumiditySetpoint;1]))");
		ParserTests.CheckType<TokenExpressionFunctionCall>(expr1);
		TokenExpressionFunctionCall func1 = ((TokenExpressionFunctionCall)expr1);
		func1.FunctionName.Should().Be("OPTION");
		func1.Children.Length.Should().Be(1);
		var firstChild = func1.Children.First() as TokenExpressionFailed;
		firstChild.ToString().Should().Be("FAILED(dtmi:com:willowinc:AirHumiditySetpoint;1)");
		firstChild.Children.Length.Should().Be(1);
	}

	[TestMethod]
	public void CanParseNumericExpressions()
	{
		//TODO: add %, ! operators
		TokenExpression expr1 = Parser.Deserialize("1 + 2 + 3 + B");
		ParserTests.CheckType<TokenExpressionAdd>(expr1);
		((TokenExpressionAdd)expr1).ToString().Should().Be("(((1 + 2) + 3) + B)");
		ParserTests.CheckSerialized(expr1, "1 + 2 + 3 + B");

		TokenExpression expr2 = Parser.Deserialize("1 + 5 * 6 / A - 2 * 3");
		ParserTests.CheckType<TokenExpressionSubtract>(expr2);
		expr2.ToString().Should().Be("((1 + ((5 * 6) / A)) - (2 * 3))");
		ParserTests.CheckSerialized(expr2, "(1 + (5 * 6) / A) - 2 * 3");

		TokenExpression expr3 = Parser.Deserialize("1 + 2 - 3 * 4 / 5 ^ 6");
		ParserTests.CheckType<TokenExpressionSubtract>(expr3);
		expr3.ToString().Should().Be("((1 + 2) - ((3 * 4) / (5^6)))");
		ParserTests.CheckSerialized(expr3, "(1 + 2) - (3 * 4) / 5^6");
	}

	[TestMethod]
	public void CanParsePowerExpression()
	{
		TokenExpression expr1 = Parser.Deserialize("A^3");
		ParserTests.CheckType<TokenExpressionPower>(expr1);
		expr1.ToString().Should().Be("(A^3)");
		ParserTests.CheckSerialized(expr1, "A^3");
	}

	[TestMethod]
	public void CanParseComparisonExpressions()
	{
		TokenExpression expr1 = Parser.Deserialize("A > 23");
		ParserTests.CheckType<TokenExpressionGreater>(expr1);
		TokenExpressionGreater inequality1 = (TokenExpressionGreater)expr1;
		inequality1.Left.Serialize().Should().Be("A");
		inequality1.Right.Serialize().Should().Be("23");
		ParserTests.CheckSerialized(expr1, "A > 23");

		TokenExpression expr2 = Parser.Deserialize("B <= 27");
		ParserTests.CheckType<TokenExpressionLessOrEqual>(expr2);
		TokenExpressionLessOrEqual inequality2 = (TokenExpressionLessOrEqual)expr2;
		inequality2.Left.Serialize().Should().Be("B");
		inequality2.Right.Serialize().Should().Be("27");
		ParserTests.CheckSerialized(expr2, "B <= 27");

		TokenExpression expr3 = Parser.Deserialize("'abc' >= 0.5");
		ParserTests.CheckType<TokenExpressionGreaterOrEqual>(expr3);
		TokenExpressionGreaterOrEqual inequality3 = (TokenExpressionGreaterOrEqual)expr3;
		inequality3.Left.Serialize().Should().Be("\"abc\"");
		inequality3.Right.Serialize().Should().Be("0.5");
		ParserTests.CheckSerialized(expr3, "\"abc\" >= 0.5");
	}

	[TestMethod]
	public void CanParseLogicalExpressions()
	{
		//TODO: make NOT a synonym for !
		TokenExpression expr1 = Parser.Deserialize("(A & B) OR !C");
		ParserTests.CheckType<TokenExpressionOr>(expr1);
		TokenExpressionOr logical1 = (TokenExpressionOr)expr1;
		logical1.UnboundVariables.Count().Should().Be(3);
		expr1.Serialize().Should().Be("(A & B) | !C");

		TokenExpression expr2 = Parser.Deserialize("!( A OR !( B & C OR D))");
		ParserTests.CheckType<TokenExpressionNot>(expr2);
		TokenExpressionNot logical2 = (TokenExpressionNot)expr2;
		logical2.UnboundVariables.Count().Should().Be(4);
		expr2.Serialize().Should().Be("!(A | !((B & C) | D))");

		TokenExpression expr3 = Parser.Deserialize("a < 5 AND b >= 6");
		ParserTests.CheckType<TokenExpressionAnd>(expr3);
		TokenExpressionAnd logical3 = (TokenExpressionAnd)expr3;
		logical3.UnboundVariables.Count().Should().Be(2);
		expr3.Serialize().Should().Be("a < 5 & b >= 6");
	}

	[TestMethod]
	public void RealExample_CheckPrecedence()
	{
		string expr = "([air_flow_sp_ratio] > 1.1) & [damper_cmd] < 0.05";
		TokenExpression exp = Parser.Deserialize(expr);
		exp.UnboundVariables.Count().Should().Be(2);
		exp.Should().BeOfType<TokenExpressionAnd>();
		exp.Serialize().Should().Be("air_flow_sp_ratio > 1.1 & damper_cmd < 0.05");
	}

	[TestMethod]
	public void CanParseArrays()
	{
		TokenExpression expr1 = Parser.Deserialize("{1, 2, 3}");
		ParserTests.CheckType<TokenExpressionArray>(expr1);
		TokenExpressionArray array = (TokenExpressionArray)expr1;
		array.UnboundVariables.Count().Should().Be(0);
		ParserTests.CheckSerialized(expr1, "{1,2,3}");
	}

	[TestMethod]
	public void CanParseFunctionOnArrays()
	{
		TokenExpression expr1 = Parser.Deserialize("AVERAGE({1, 2, 3})");
		ParserTests.CheckType<TokenExpressionAverage>(expr1);
		ParserTests.CheckSerialized(expr1, "AVERAGE({1,2,3})");
	}

	[TestMethod]
	public void CanParseMultipleMultipliers()
	{
		TokenExpression expr1 = Parser.Deserialize("a * b * c + 2");
		expr1.Serialize().Should().Be("a * b * c + 2");
	}

	[TestMethod]
	public void CanParseFunctionOnArraysWithTimePeriodAsExpression()
	{
		TokenExpression expr1 = Parser.Deserialize("AVERAGE({1, 2, 3}, (5 + 5)h)");
		var average = expr1 as TokenExpressionTemporal;
		average.Should().NotBeNull();
		(average.TimePeriod is TokenExpressionAdd).Should().BeTrue();
		average.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void CanParseFunctionOnArraysWithTimePeriod()
	{
		TokenExpression expr1 = Parser.Deserialize("AVERAGE({1, 2, 3}, 5h)");
		var average = expr1 as TokenExpressionTemporal;
		average.Should().NotBeNull();
		(average.TimePeriod as TokenDouble).Value.Should().Be(5);
		average.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void CanParseDelta()
	{
		TokenExpression expr1 = Parser.Deserialize("DELTA({1, 2, 3}, 5h)");
		var delta = expr1 as TokenExpressionTemporal;
		delta.Should().NotBeNull();
		(delta.TimePeriod as TokenDouble).Value.Should().Be(5);
		delta.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void TokenExpressionStandardDeviation()
	{
		TokenExpression expr1 = Parser.Deserialize("STND({6, 2, 3, 1}, 5h)");
		var stnd = expr1 as TokenExpressionTemporal;
		stnd.Should().NotBeNull();
		(stnd.TimePeriod as TokenDouble).Value.Should().Be(5);
		stnd.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void TokenExpressionSlope()
	{
		TokenExpression expr1 = Parser.Deserialize("SLOPE({6, 2, 3, 1}, 5h)");
		var slope = expr1 as TokenExpressionTemporal;
		slope.Should().NotBeNull();
		(slope.TimePeriod as TokenDouble).Value.Should().Be(5);
		slope.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void TokenExpressionForecast()
	{
		TokenExpression expr1 = Parser.Deserialize("FORECAST({6, 2, 3, 1}, 5h)");
		var fc = expr1 as TokenExpressionTemporal;
		fc.Should().NotBeNull();
		(fc.TimePeriod as TokenDouble).Value.Should().Be(5);
		fc.TimePeriod.Unit.Should().Be("h");
	}

	[TestMethod]
	public void CanParseDualPropertyAccess()
	{
		TokenExpression expr1 = Parser.Deserialize("this.supplyFan.motorPower");
		expr1.Serialize().Should().Be("(this.supplyFan).motorPower");
		expr1.Should().BeOfType<TokenExpressionPropertyAccess>();
		var pa = expr1 as TokenExpressionPropertyAccess;
		pa.PropertyName.Should().Be("motorPower");

		pa.Child.Should().BeOfType<TokenExpressionPropertyAccess>();
		var pa2 = pa.Child as TokenExpressionPropertyAccess;
		pa2.PropertyName.Should().Be("supplyFan");
		pa2.Child.Serialize().Should().Be("this");
	}

	[TestMethod]
	public void CanParseComplexDualPropertyAccess()
	{
		TokenExpression expr1 = Parser.Deserialize("this.supplyFan.motorPower * [fan_speed]^3");
		expr1.Serialize().Should().Be("(this.supplyFan).motorPower * fan_speed^3");
	}

	[TestMethod]
	public void CanParseComplexObjectVariableAccessValid()
	{
		TokenExpression expr1 = Parser.Deserialize("(this.supplyFan).motorPower * [fan_speed]^3");
		ParserTests.CheckType<TokenExpressionMultiply>(expr1);
		expr1.Serialize().Should().Be("(this.supplyFan).motorPower * fan_speed^3");
	}

	[TestMethod]
	[Timeout(100)]
	public void CanParseManyPlusInTimelyFashion()
	{
		TokenExpression expr1 = Parser.Deserialize("[p1] + [p2] + [p3] + [p4] + [p5] + [p6] + [p7] + [p8] + [p9] + [p10] + [p11] + [p12] + [p13] + [p14] + [p15] + [p16] + [p17] + [p18] + [p19]");
		expr1.Serialize().Should().Be("p1 + p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9 + p10 + p11 + p12 + p13 + p14 + p15 + p16 + p17 + p18 + p19");
	}

	/// <summary>
	/// Test all expressions in the rules library
	/// </summary>
	/// <remarks>
	/// Add additional expressions to PointExpressions.txt or update using zip file
	/// </remarks>
	[TestMethod]
	public void CanParseAllExpressionsFromFile()
	{
		var textFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "PointExpressions.txt");
		var zipFilePath = Path.Combine(Environment.CurrentDirectory, "Data", "TwinPlatform-RulesLibrary-main.zip");

		if (Path.Exists(zipFilePath))
		{
			DataHelper.TryReadRulesFromZip(zipFilePath, out var rules);
			rules.Count.Should().BeGreaterThan(0);
			DataHelper.WritePointExpressionsToFile(textFilePath, rules);
			//Note: Copy the latest file from the bin folder and replace at the physical location if source was updated.
		}

		if (!Path.Exists(textFilePath)) throw new FileNotFoundException(textFilePath);

		DataHelper.TryReadPointExpressions(textFilePath, out var pointExpressions);

		pointExpressions.Count().Should().BeGreaterThan(0);

		foreach (var expression in pointExpressions)
		{
			try
			{
				var expressionParsed = Parser.Deserialize(expression);
				var serialized = expressionParsed.Serialize();
				var serialized1 = Parser.Deserialize(serialized).Serialize();
				serialized.Should().Be(serialized1);
			}
			catch (ParserException)
			{
				Assert.Fail($"Parser failed for {expression}", expression);
			}
		}
	}
}

#pragma warning restore CS8602 // Dereference of a possibly null reference.
