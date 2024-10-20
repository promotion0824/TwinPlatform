using Abodit.Mutable;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using WillowRules.Visitors;
using Willow.Rules.Model;
using Willow.Rules.Services;

namespace Willow.Rules.Test;

[TestClass]
public class UnitsVisitorTests
{
	[TestMethod]
	public void CanVisitAddForSameUnit()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionAdd(
			TokenExpressionConstant.Create(23, "kWh"),
			TokenExpressionConstant.Create(19, "kWh"));

		visitor.Visit(expression).Should().Be("kWh");
	}

	[TestMethod]
	public void CanVisitAddForDifferentUnit()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionAdd(
			TokenExpressionConstant.Create(23, "kWh"),
			TokenExpressionConstant.Create(19, "km"));

		visitor.Visit(expression).Should().Be("error");
	}

	[TestMethod]
	public void CanVisitSubtractForSameUnit()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionSubtract(
			TokenExpressionConstant.Create(23, "kWh"),
			TokenExpressionConstant.Create(19, "kWh"));

		// At some point we want to add the concept of a delta unit so that deltas are graphed separately

		visitor.Visit(expression).Should().Be("kWh");
	}

	[TestMethod]
	public void CanVisitDivideForSameUnit()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionDivide(
			TokenExpressionConstant.Create(23, "kWh"),
			TokenExpressionConstant.Create(19, "kWh"));

		visitor.Visit(expression).Should().Be("");
	}

	[TestMethod]
	public void CanVisitMultiplyWithScalar()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionMultiply(
			TokenExpressionConstant.Create(23, ""),
			TokenExpressionConstant.Create(19, "kWh"));

		visitor.Visit(expression).Should().Be("kWh");
	}

	[TestMethod]
	public void CanVisitComparisonToGetBool()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionGreater(
			TokenExpressionConstant.Create(23, "kWh"),
			TokenExpressionConstant.Create(19, "kWh"));

		visitor.Visit(expression).Should().Be("bool");
	}

	[TestMethod]
	public void CanVisitMismatchedComparisonToGetError()
	{
		var visitor = new UnitsVisitor();
		var expression = new TokenExpressionGreater(
			TokenExpressionConstant.Create(23, "degC"),
			TokenExpressionConstant.Create(19, "degF"));

		visitor.Visit(expression).Should().Be("error");
	}

	[TestMethod]
	[DataRow("FAHRENHEIT", "degZ", "degZ")]  // didn't recognize internal unit, can't convert
	[DataRow("FAHRENHEIT", "degC", "degF")]
	[DataRow("CELSIUS", "degF", "degC")]
	public void FahrenheitCelciusFunctionsReturnsRightUnit(string functionName, string internalUnit, string resultUnit)
	{
		var poco = new BasicDigitalTwinPoco();

		var bindVisitor = new BindToTwinsVisitor(
			Env.Empty.Push(),
			poco,
			Mock.Of<IMemoryCache>(),
			Mock.Of<IModelService>(),
			Mock.Of<ITwinService>(),
			Mock.Of<ITwinSystemService>(),
			Mock.Of<IMLService>(),
			Mock.Of<ILogger<BindToTwinsVisitor>>());

		var visitor = new UnitsVisitor();

		var expression = new TokenExpressionFunctionCall(functionName, typeof(double),
			TokenExpressionConstant.Create(23, internalUnit));

		var bound = bindVisitor.Visit(expression);
		bound.Unit.Should().Be(resultUnit);

		var final = visitor.Visit(bound);
		final.Should().Be(resultUnit);
	}

}
