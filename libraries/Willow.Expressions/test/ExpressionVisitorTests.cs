using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace WillowExpressions.Test;

#pragma warning disable SA1027

[TestClass]
public class ExpressionVisitorTests
{
    private class AirHandler
    {
        public double returnairtemperature { get; set; }
        public double returnairsetpoint { get; set; }
    }

    private class SampleExpressionVisitor : Willow.Expressions.Visitor.TokenExpressionVisitor
    {
        /// <summary>
        /// Example of a transformation we might do as a visit: flip `NOT(a&gt;b)` to `a&lt;=b`
        /// </summary>
        public override Willow.Expressions.TokenExpression DoVisit(TokenExpressionNot input)
        {
            if (input.Child is TokenExpressionGreater tg)
            { return new TokenExpressionLessOrEqual(tg.Left.Accept(this), tg.Right.Accept(this)); }
            else
            {
                return base.DoVisit(input);
            }
        }
    }

    [TestMethod]
    public void CanRewriteNotExpressionUsingVisitor()
    {
        var expr = Parser.Deserialize("!(a > b)");

        var visitor = new SampleExpressionVisitor();
        var result = visitor.Visit(expr);

        result.ToString().Should().Be("(a <= b)");
    }

    [TestMethod]
    public void CanSimplifyNumericExpressionUsingVisitor()
    {
        SimplifyExpression("5 * 5", "25");
        SimplifyExpression("8 + 4 / 2 - 10", "0");
        SimplifyExpression("4 / 5 * 4 ^ -1", "0.2");
        SimplifyExpression("A - 18 / (2 + 1)", "A + -6");
    }

    [TestMethod]
    public void CanInvertFunctionUsingVisitor()
    {
        InvertFunction("f(x) = y / 3", "f(x)", "y", "f(x) == f(x) * 3");
    }

    [DataRow("x ^ 2", "2 * x^1 * 1")]
    [DataRow("2 ^ x", "2^x * ln(2) * 1")]
    [DataRow("x ^ 3 + x ^ 2 + x ^ 1", "3 * x^2 * 1 + 2 * x^1 * 1 + 1")]
    [DataRow("5 * x ^ 2", "0 * x^2 + 2 * x^1 * 1 * 5")]
    [DataRow("x ^ 3 * x ^ 2", "3 * x^2 * 1 * x^2 + 2 * x^1 * 1 * x^3")]
    [DataRow("x * x ^ 2 * x ^ 3", "(1 * x^2 + 2 * x^1 * 1 * x) * x^3 + 3 * x^2 * 1 * x * x^2")] // 6x^5 simplified

    [TestMethod]
    public void CanDifferentiateNumericExpressionUsingVisitor(string input, string result)
    {
        TokenExpressionVariableAccess x = new("x");
        TokenExpressionDifferentiateVisitor visitor = new(x);

        TokenExpression expr1 = Parser.Deserialize(input);
        TokenExpression derivative1 = visitor.Visit(expr1);
        derivative1.Serialize().Should().Be(result);
    }

    private void InvertFunction(string expression, string function, string variable, string inverted)
    {
        TokenExpressionInvertVisitor visitor = new TokenExpressionInvertVisitor(Parser.Deserialize(function), (TokenExpressionVariableAccess)Parser.Deserialize(variable));
        TokenExpression result = visitor.DoVisit((TokenExpressionEquals)Parser.Deserialize(expression));
        CheckSerialized(result, inverted);
    }

    private void SimplifyExpression(string original, string simplified)
    {
        TokenExpression expr = Parser.Deserialize(original);
        TokenExpressionSimplifier visitor = new TokenExpressionSimplifier();
        TokenExpression result = visitor.Visit(expr);
        CheckSerialized(result, simplified);
    }

    private void CheckSerialized(TokenExpression expression, string str)
    {
        expression.Serialize().Should().Be(str);
    }
}

[TestClass]
public class ConvertToExpressionTests
{
    private class AirHandler
    {
        public double Returnairtemperature { get; set; }
        public double Returnairsetpoint { get; set; }
    }

    ConvertToValueVisitor<Env> getVisitor(Env env) => new ConvertToValueVisitor<Env>(env, (x, s) => x.Get(s));

    [TestMethod]
    public void CanDeductDates()
    {
        var expr = Parser.Deserialize("([date1] - [date2]).TotalDays");

        var env = Env.Empty.Push();
        var date1 = new DateTime(2000, 01, 03);
        var date2 = new DateTime(2000, 01, 01);

        env.Assign("date1", date1);
        env.Assign("date2", date2);

        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToDouble(null).Should().Be(2);
    }

    [TestMethod]
    public void CanAccessJsonProperty()
    {
        var expr = Parser.Deserialize("[sensor].myprop.other == 10 && [sensor].mystr == 'testing' && [sensor].date.Hour == 16 && [sensor].boolValue == true");

        var json = """
				{
					"myprop": {
						"other":10
					},
					"mystr":"testing",
					"boolValue": true,
					"date":"2024-02-23T16:09:51.9978022"
				}
				""";

        var env = Env.Empty.Push();
        env.Assign("sensor", json);

        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToBoolean(null).Should().BeTrue();
    }

    [TestMethod]
    public void CanDoMath()
    {
        var expr = Parser.Deserialize("A + B");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 23.2);

        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.Should().Be(35.2);
    }

    [DataRow(70, 65, 75, 0)]
    [DataRow(60, 65, 75, 5)]
    [DataRow(80, 65, 75, 5)]
    [TestMethod]
    public void CanDoDeadband(double input, double min, double max, double expected)
    {
        var expr = Parser.Deserialize($"DEADBAND(input, {min}, {max})");

        var env = Env.Empty.Push();
        env.Assign("input", input);

        var visitor = getVisitor(env);
        var result = visitor.Visit(expr);

        result.Should().Be(expected);
    }

    [TestMethod]
    public void MaxWorks()
    {
        var expr = Parser.Deserialize("MAX({A, B, C})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(35.2);
    }

    [TestMethod]
    public void CanParseFunctionOnArraysWithTimePeriod1()
    {
        double avg = 1;
        var temporal = new TemporalMock()
        {
            AverageResult = avg,
        };
        TokenExpression expr = Parser.Deserialize("AVERAGE([A], (5 + 5)h)");
        var env = Env.Empty.Push();
        env.Assign("A", temporal);
        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToDouble(null).Should().Be(avg);
        temporal.AveragePeriod.Unit.Should().Be(Unit.hour);
        temporal.AveragePeriod.Value.Should().Be(10);
    }

    [TestMethod]
    public void CanParseUnitWithOddCharacters()
    {
        TokenExpression expr = Parser.Deserialize("5[in.wc]");
        var env = Env.Empty.Push();
        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToDouble(null).Should().Be(5);
        expr.Unit.Should().Be("in.wc");
    }

    [TestMethod]
    public void CanParseTemporalWithDateRange()
    {
        double avg = 1;
        var temporal = new TemporalMock()
        {
            AverageResult = avg,
        };
        TokenExpression expr = Parser.Deserialize("AVERAGE([A], (5 + 5)h, (15 + 5)h)");
        var env = Env.Empty.Push();
        env.Assign("A", temporal);
        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToDouble(null).Should().Be(avg);
        temporal.AveragePeriod.Value.Should().Be(10);
        temporal.AverageFrom.Value.Should().Be(20);
    }

    [TestMethod]
    public void CanParseTemporalWithAlias()
    {
        double avg = 1;
        var temporal = new TemporalMock()
        {
            AverageResult = avg,
        };
        TokenExpression expr = Parser.Deserialize("AVERAGE([A], (5 + 5)hours, (15 + 5)hr)");
        var env = Env.Empty.Push();
        env.Assign("A", temporal);
        var visitor = getVisitor(env);
        var dotnetexpr = visitor.Visit(expr);

        dotnetexpr.ToDouble(null).Should().Be(avg);
        temporal.AveragePeriod.Value.Should().Be(10);
        temporal.AverageFrom.Value.Should().Be(20);
    }

    [TestMethod]
    public void MaxWorksOnOne()
    {
        var expr = Parser.Deserialize("MAX(A)");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(12.0);
    }

    [TestMethod]
    public void MinWorks()
    {
        var expr = Parser.Deserialize("MIN({A, B, C})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(12.0);
    }

    [TestMethod]
    public void AllWorks()
    {
        var expr = Parser.Deserialize("ALL({A>0, B>0, C>0})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(true);
    }

    [TestMethod]
    public void AllWorksFalse()
    {
        var expr = Parser.Deserialize("ALL({A>0, B<0, C>0})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(false);
    }

    [TestMethod]
    public void AnyWorks()
    {
        var expr = Parser.Deserialize("ANY({A>0, B>0, C>0})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(true);
    }

    [TestMethod]
    public void AnyWorksFalse()
    {
        var expr = Parser.Deserialize("ANY({A<0, B<0, C<0})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(false);
    }

    [TestMethod]
    public void CountShouldCountTrueValues()
    {
        var expr = Parser.Deserialize("COUNT({A>0, B<0, C>0})");

        var env = Env.Empty.Push();
        env.Assign("A", 12.0);
        env.Assign("B", 35.2);
        env.Assign("C", 18.1);

        var visitor = getVisitor(env);
        var convertibleValue = visitor.Visit(expr);

        convertibleValue.Should().Be(2);
    }

    [TestMethod]
    public void TemporalExpressionMustBindToVariableName()
    {
        var expr = Parser.Deserialize("MIN(a, 1h, -1h)");
        expr.Serialize().Should().Be("MIN(a, 1[h], (-1[h]))");
    }

    [TestMethod]
    [DataRow("5kWh * 1.8", "5[kWh] * 1.8", "kWh")]
    [DataRow("5USD + 2USD", "5[USD] + 2[USD]", "USD")]
    [DataRow("5W * 10s", "5[W] * 10[s]", "W.s")]
    public void CanParseNumberExpressionWithUnits(string expr, string expectedExpression, string expectedUnit)
    {
        TokenExpression expr1 = Parser.Deserialize(expr);
        var unit = new UnitsVisitor().Visit(expr1);
        unit.Should().Be(expectedUnit);
        expr1.Serialize().Should().Be(expectedExpression);
    }
}
