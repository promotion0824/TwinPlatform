using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace WillowExpressions.Test;

[TestClass]
public class SerializationTests
{
    [TestMethod]
    public void TestScientificNotationAvoidance()
    {
        var expr = new TokenDouble(0.00000005);
        expr.Serialize().Should().Be("0.00000005");
    }

    [DataRow("1+2", "1 + 2")]
    [DataRow("1+2*3", "1 + 2 * 3")]
    [DataRow("(1+2)*3", "(1 + 2) * 3")]
    [DataRow("27 < 9 | 19 > 4", "27 < 9 | 19 > 4")]
    [DataRow("(1+2)^(3+4)", "(1 + 2)^(3 + 4)")]
    [DataRow("(((((1+2)))))", "1 + 2")]
    [DataRow("(1+(2+(3+(4+(5+6)))))", "1 + 2 + 3 + 4 + 5 + 6")]
    [DataRow("(1 + 2 + 3 + 4 + 5 + 6)", "1 + 2 + 3 + 4 + 5 + 6")]
    [DataRow("!(A | B)", "!(A | B)")]
    [DataRow("(!A) | B", "!A | B")]
    [DataRow("(E + F).LENGTH", "(E + F).LENGTH")]
    [DataRow("E + F.LENGTH", "E + F.LENGTH")]
    [DataRow("(-a) + (-b)", "-a + -b")]
    [DataRow("(-a) + (-b)", "-a + -b")]
    [DataRow("ABS(a) * ABS(b)", "ABS(a) * ABS(b)")]
    [DataRow("a^3 + b^2 + c", "a^3 + b^2 + c")]
    [DataRow("a^(-3) + b^(-2) + c", "a^(-3) + b^(-2) + c")]
    [DataRow("(a+b+c)^3", "(a + b + c)^3")]
    [DataRow("(a+b+c)^-3", "(a + b + c)^(-3)")]
    [DataRow("(a+b)/(c*d)", "(a + b) / (c * d)")]
    [DataRow("(a+b)/(c*d).e", "(a + b) / (c * d).e")]
    [DataRow("a/b/c/d", "((a / b) / c) / d")]
    [DataRow("a*b*c*d", "a * b * c * d")]
    [DataRow("IF(1!=2,1,0)", "IF(1 != 2, 1, 0)")]
    [DataRow("0.8 * DELTA(Cost, 7d) / (7 * 24) + 0.2 * DELTA(Cost, 31d) / (24 * 31)", "(0.8 * DELTA(Cost, 7[d])) / (7 * 24) + (0.2 * DELTA(Cost, 31[d])) / (24 * 31)")]
    [DataRow("0.8 * DELTA(Cost, (1 + 3)d) / (7 * 24) + 0.2 * DELTA(Cost, 31d) / (24 * 31)", "(0.8 * DELTA(Cost, (1 + 3)[d])) / (7 * 24) + (0.2 * DELTA(Cost, 31[d])) / (24 * 31)")]
    [TestMethod]
    public void RoundTrip(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        expr.Serialize().Should().Be(expected);
    }

    [DataRow("!A | B", "!A | B")]
    [DataRow("C | !D", "C | !D")]
    [TestMethod]
    public void UnaryNot(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        expr.Serialize().Should().Be(expected);
    }

    [DataRow("-a + b", "-a + b")]
    [DataRow("a + -b", "a + -b")] // DOES NOT WORK
    [TestMethod]
    public void UnaryMinus(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        expr.Serialize().Should().Be(expected);
    }

    [DataRow("a^-3 + b^-2 + c", "a^(-3) + b^(-2) + c")]
    [DataRow("(a+b+c)^-3", "(a + b + c)^(-3)")]
    [TestMethod]
    public void UnaryMinusPower(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        expr.Serialize().Should().Be(expected);
    }

    [DataRow("[a;1] - [b;1] + c", "[a;1] - [b;1] + c")]
    [DataRow("[a;1].[b;1] + c", "[a;1].[b;1] + c")]
    [DataRow("[a].[b;1] + c", "a.[b;1] + c")]
    [DataRow("[a;1].[b] + c", "[a;1].b + c")]
    [TestMethod]
    public void SquaredVariables(string input, string expected)
    {
        var expr = Parser.Deserialize(input);
        expr.Serialize().Should().Be(expected);
    }

}
