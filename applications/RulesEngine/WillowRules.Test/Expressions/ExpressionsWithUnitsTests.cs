using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;

namespace WillowExpressions.Test;

[TestClass]
public class ExpressionsWithUnitsTests
{
	private void CheckType<T>(TokenExpression expression)
	{
		expression.Should().BeOfType<T>();
	}

	private void CheckSerialized(TokenExpression expression, string str)
	{
		expression.Serialize().Should().Be(str);
	}

	[TestMethod]
	[DataRow("kWh", "[kWh]")]
	[DataRow("iwc", "[iwc]")]
	[DataRow("Pa", "[Pa]")]
	[DataRow("kPa", "[kPa]")]
	[DataRow("W", "[W]")]
	[DataRow("kW", "[kW]")]
	[DataRow("%", "[%]")]
	[DataRow("USD", "[USD]")]
	[DataRow("$", "[$]")]
	public void CanParseNumbersWithUnits(string unit, string expectedUnit)
	{
		TokenExpression expr1 = Parser.Deserialize("21.1" + unit);
		expr1.Unit.Should().Be(unit);
		CheckSerialized(expr1, "21.1" + expectedUnit);
	}

	UnitsVisitor unitVisitor = new UnitsVisitor();

	[TestMethod]
	[DataRow("5kWh * 1.8", "5[kWh] * 1.8", "kWh")]
	[DataRow("5USD + 2USD", "5[USD] + 2[USD]", "USD")]
	[DataRow("5W * 10s", "5[W] * 10[s]", "W.s")]
	public void CanParseNumberExpressionWithUnits(string expr, string expectedExpression, string expectedUnit)
	{
		TokenExpression expr1 = Parser.Deserialize(expr);
		string unit = unitVisitor.Visit(expr1);
		unit.Should().Be(expectedUnit);
		expr1.Serialize().Should().Be(expectedExpression);
	}

	//
	// Prefix units of measure are not supported
	//
	// [TestMethod]
	// [DataRow("$")]
	// public void CanParsePrefixUnits(string unit)
	// {
	// 	TokenExpression expr1 = Parser.Deserialize(unit + "21.1");
	// 	expr1.Unit.Should().Be(unit);
	// 	CheckSerialized(expr1, unit + "21.1");
	// }

	[TestMethod]
	public void UnitDateTimeSnapToDateTime()
	{
		var offset = TimeSpan.FromHours(1);
		var date = new DateTimeOffset(new DateTime(2024, 6, 1, 10, 15, 30), offset);

		var result = Unit.Get("Mth").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 5, 1), offset));

		result = Unit.Get("wk").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 5, 12), offset));

		result = Unit.Get("d").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 5, 30), offset));

		result = Unit.Get("h").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 6, 1, 8, 0, 0), offset));

		result = Unit.Get("m").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 6, 1, 10, 13, 0), offset));

		result = Unit.Get("s").SnapToDateTime(-2, date);
		result.Should().Be(new DateTimeOffset(new DateTime(2024, 6, 1, 10, 15, 28), offset));
	}

	[TestMethod]
	public void UnitDateTimeGetTimeSpanDuration()
	{
		var now = DateTimeOffset.Now;

		var result = Unit.month.GetTimeSpanDuration(1, now);
		result.Should().Be(now - now.AddMonths(-1));

		result = Unit.week.GetTimeSpanDuration(1, DateTimeOffset.Now);
		result.Should().Be(TimeSpan.FromDays(7));

		result = Unit.day.GetTimeSpanDuration(2, DateTimeOffset.Now);
		result.Should().Be(TimeSpan.FromDays(2));

		result = Unit.minute.GetTimeSpanDuration(30, DateTimeOffset.Now);
		result.Should().Be(TimeSpan.FromMinutes(30));

		result = Unit.hour.GetTimeSpanDuration(3, DateTimeOffset.Now);
		result.Should().Be(TimeSpan.FromHours(3));
	}
}
