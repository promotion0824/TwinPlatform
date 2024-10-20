using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;

namespace WillowExpressions.Test;

[TestClass]
public class EvaluationTests
{
    [TestMethod]
    [DataRow("\"B\"", "\"B\"")]
    [DataRow("\"B\'B\"", "\"B'B\"")]
    [DataRow("\"\\\\C\"", "\"\\C\"")]
    public void CanHandleEscapeCharacters(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        string serialized = expr.Serialize();
        serialized.Should().Be(expected);
    }

    [TestMethod]
    public void CanCalculateSimpleFormula()
    {
        var expr = Parser.Deserialize("5+12");
        Env env = Env.Empty.Push();
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(17);
    }

    [TestMethod]
    public void CanCalculateSimpleDivision()
    {
        var expr = Parser.Deserialize("12/4");
        Env env = Env.Empty.Push();
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    [TestMethod]
    [DataRow(16384, 256, "a/b", 64)]
    [DataRow(238, 0, "a^3 + a^2 + a", 13538154)]
    public void CanCalculateFromEnvironment(double a, double b, string expression, double expected)
    {
        var expr = Parser.Deserialize(expression);
        Env env = Env.Empty.Push();
        env.Assign("a", a);
        env.Assign("b", b);
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    [DataRow(268)]
    [DataRow(2000)]
    [DataRow(20000)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(329.9)]
    [DataRow(832.6)]
    public void CustomerTestCaseForCalcPoint(double a)
    {
        string expression = "IF([HP-SOFI-ST-L01-CH-1-ElecCurrentSensor-53AI3001283] == 0, 0, (2.5737056*10^(-6))*[HP-SOFI-ST-L01-CH-1-ElecCurrentSensor-53AI3001283]^3-0.004442847*[HP-SOFI-ST-L01-CH-1-ElecCurrentSensor-53AI3001283]^2+3.4960746*[HP-SOFI-ST-L01-CH-1-ElecCurrentSensor-53AI3001283]-709.91209)";
        var expr = Parser.Deserialize(expression);
        Env env = Env.Empty.Push();
        env.Assign("HP-SOFI-ST-L01-CH-1-ElecCurrentSensor-53AI3001283", a);
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();

        double calculated = a == 0 ? 0.0 : (2.5737056 * Math.Pow(10, -6)) * Math.Pow(a, 3) - 0.004442847 * Math.Pow(a, 2) + 3.4960746 * a - 709.91209;
        Console.WriteLine($"{a} -> {result.Value} == {calculated}");
        result.Value.Should().Be(calculated);

        if (a > 300)
        {
            result.Value.ToDouble(CultureInfo.InvariantCulture).Should().BeGreaterThan(0);
        }
    }


    [TestMethod]
    public void CanCalculateSimpleMath()
    {
        var expr = Parser.Deserialize("atan(5)");
        Env env = Env.Empty.Push();
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.Should().BeOfType<double>();
        result.Value.ToDouble(CultureInfo.InvariantCulture).Should().BeApproximately(1.373400766945016, 0.0001);
    }

    [TestMethod]
    public void CanCalculateWetbulbExpression()
    {
        var expr = Parser.Deserialize("0.151977 * (rh + 8.313659)^(1/2)");
        Env env = Env.Empty.Push();
        env.Assign("rh", 56);
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.Should().BeOfType<double>();
        result.Value.ToDouble(CultureInfo.InvariantCulture).Should().BeApproximately(1.2187916681919835, 0.0001);
    }

    [TestMethod]
    public void CanParseWetbulbExpressionWithEmptyEnv()
    {
        ParserEnvironment env = new();
        var expr = Parser.Deserialize("0.151977 * (rh + 8.313659)^(1/2)", env);
        string ser = expr.Serialize().Replace(" ", string.Empty);
        ser.Should().Be("0.151977*(rh+8.313659)^(1/2)");
    }
    [TestMethod]
    public void CanParseComplexWetbulbExpressionWithEmptyEnv()
    {
        ParserEnvironment env = new();
        var expr = Parser.Deserialize("T * arctan(0.151977 * (rh + 8.313659)^(1/2)) + arctan(T + rh) - arctan(rh - 1.676331) + 0.00391838 *(rh)^(3/2) * arctan(0.023101 * rh) - 4.686035", env);
        string ser = expr.Serialize().Replace(" ", string.Empty);
        ser.Should().Be("((T*arctan(0.151977*(rh+8.313659)^(1/2))+arctan(T+rh))-arctan(rh-1.676331)+0.00391838*rh^(3/2)*arctan(0.023101*rh))-4.686035");
    }

    [TestMethod]
    public void CanParsePOW()
    {
        Env env = Env.Empty.Push();
        var expr = Parser.Deserialize("POW(10, 2)");
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.ToDouble(CultureInfo.InvariantCulture).Should().Be(100);
    }

    [TestMethod]
    [DataRow("HOUR(a)", 4)]
    [DataRow("MINUTE(a)", 5)]
    [DataRow("DAY(a)", 3)]
    [DataRow("DAYOFWEEK(a)", 3)]
    [DataRow("MONTH(a)", 2)]
    public void TimeFunctions(string expression, double assert)
    {
        var date = new DateTime(2010, 2, 3, 4, 5, 0, 0, 0, DateTimeKind.Utc);

        var expr = Parser.Deserialize(expression);
        Env env = Env.Empty.Push();
        env.Assign("a", date);
        var result = expr.EvaluateDirectUsingEnv(env);
        result.HasValue.Should().BeTrue();
        result.Value.ToDouble(CultureInfo.InvariantCulture).Should().Be(assert);
    }

    [TestMethod]
    public void CanOptimiseExpression1()
    {
        var expr = Parser.Deserialize("1 + 1");
        Env env = Env.Empty.Push();
        var visitor = new ConstOptimizerVisitor(env);
        var result = visitor.Visit(expr);
        result.Serialize().Should().Be("2");
    }

    [TestMethod]
    public void ConstOptimizerMustKeepUnit()
    {
        var expr = Parser.Deserialize("(1 + 1)d");
        Env env = Env.Empty.Push();
        var visitor = new ConstOptimizerVisitor(env);
        var result = visitor.Visit(expr);
        result.Serialize().Should().Be("2[d]");
        result.Unit.Should().Be("d");
    }

    [TestMethod]
    public void ConstOptimizerMustKeepUnit1()
    {
        var expr = Parser.Deserialize("-1d");
        Env env = Env.Empty.Push();
        var visitor = new ConstOptimizerVisitor(env);
        var result = visitor.Visit(expr);
        result.Serialize().Should().Be("-1[d]");
    }

    [TestMethod]
    public void CanOptimiseExpression2()
    {
        var expr = Parser.Deserialize("IF(a == 'somevalue', 1, 0) + 10");
        Env env = Env.Empty.Push();
        env.Assign("a", TokenExpressionConstant.Create("somevalue"));
        var visitor = new ConstOptimizerVisitor(env);
        var result = visitor.Visit(expr);
        result.Serialize().Should().Be("11");
    }

    [TestMethod]
    public void CanOptimiseExpression3()
    {
        var expr = Parser.Deserialize("SUM({IF(a == 'somevalue', 1, 0), 5 * 10, b}) + IF(somevariable == 1, 10 * 10, 100 - 5)");
        Env env = Env.Empty.Push();
        env.Assign("a", TokenExpressionConstant.Create("somevalue"));
        env.Assign("b", TokenExpressionConstant.Create(20));
        env.Assign("somevariable", new TokenExpressionVariableAccess("somevariable"));
        var visitor = new ConstOptimizerVisitor(env);
        var result = visitor.Visit(expr);
        result.Serialize().Should().Be("71 + (IF(somevariable == 1, 100, 95))");
    }
}
