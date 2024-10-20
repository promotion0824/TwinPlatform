using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace WillowExpressions.Test;

[TestClass]
public class OverallTests
{
    private class AirHandler
    {
        public double returnairtemperature { get; set; }
        public double returnairsetpoint { get; set; }
    }

    [TestMethod]
    public void MustSnapToTime()
    {
        var offset = TimeSpan.FromHours(1);

        //was a friday
        var date = new DateTimeOffset(new DateTime(2024, 3, 1, 10, 15, 30), offset);

        var result = Unit.Get("Mth").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 1, 1), offset));

        result = Unit.Get("wk").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 2, 11), offset));

        result = Unit.Get("d").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 2, 28), offset));

        result = Unit.Get("h").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 3, 1, 8, 0, 0), offset));

        result = Unit.Get("m").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 3, 1, 10, 13, 0), offset));

        result = Unit.Get("s").SnapToTime(-2, date);

        result.Should().Be(new DateTimeOffset(new DateTime(2024, 3, 1, 10, 15, 28), offset));
    }

    [TestMethod]
    public void UnitsLookupMustBeUnique()
    {
        var names = Unit.PredefinedUnits.Select(v => v.Name);
        names.ToDictionary(v => v);
        var aliases = Unit.PredefinedUnits.SelectMany(v => v.Aliases);
        aliases.ToDictionary(v => v);
        names.Concat(aliases).ToDictionary(v => v);
    }

    [TestMethod]
    public void TestMethod1()
    {
        var env = new ParserEnvironment();
        env.AddVariable("return air temperature", typeof(double));
        env.AddVariable("return air sp", typeof(double));

        var expr = Parser.Deserialize("[return air temperature] - [return air sp]", env);

        var b1 = expr.Bind("return air temperature", 77);
        var b2 = b1.Bind("return air sp", 70);

        b2.ToString().Should().Be("(77 - 70)");

        b2.Simplify().ToString().Should().Be("7");

        Env runtimeenv = Env.Empty.Push();
        runtimeenv.Assign("return air temperature", 77);
        runtimeenv.Assign("return air sp", 70);

        var result = expr.EvaluateDirectUsingEnv(runtimeenv);

        result.HasValue.Should().BeTrue();
        result.Value.Should().Be(7.0);

        //// This can't work with spaces in names
        //var fc = expr.Convert<AirHandler, object>();
        //var f = fc.Compile();

        //f(new AirHandler()).Should().Be(7);

        //// This can't work with spaces in names
        //var foo = new AirHandler { returnairsetpoint = 70, returnairtemperature = 77 };
        //f(foo).Should().Be(7);
    }

    [TestMethod]
    public void Get_EmptyOrNullInput_ReturnsScalar()
    {
        var result = Unit.Get(null);
        Assert.AreEqual(Unit.scalar, result);

        result = Unit.Get(string.Empty);
        Assert.AreEqual(Unit.scalar, result);
    }

    [TestMethod]
    public void Get_AliasInPredefinedUnits_ReturnsUnit()
    {
        var result = Unit.Get("hour");
        Assert.IsNotNull(result);
        Assert.AreEqual(Unit.hour, result);
    }

    [TestMethod]
    public void Get_NewUnit_CreatesAndReturnsUnit()
    {
        var result = Unit.Get("newUnit");
        Assert.IsNotNull(result);
        Assert.AreEqual("newUnit", result.Name);
    }

    [TestMethod]
    public void TryGetUnit_EmptyOrNullInput()
    {
        var result = Unit.TryGetUnit(null, out Unit unitOfMeasure);
        Assert.IsFalse(result);
        Assert.IsNull(unitOfMeasure);

        result = Unit.TryGetUnit(string.Empty, out unitOfMeasure);
        Assert.IsFalse(result);
        Assert.IsNull(unitOfMeasure);

        result = Unit.TryGetUnit("newTryGetUnit", out unitOfMeasure);
        Assert.IsFalse(result);
        Assert.IsNull(unitOfMeasure);
    }

    [TestMethod]
    public void TryGetUnit_AliasInPredefinedUnits()
    {
        var result = Unit.TryGetUnit("day", out Unit unitOfMeasure);
        Assert.IsTrue(result);
        Assert.IsNotNull(unitOfMeasure);
        Assert.AreEqual(Unit.day, unitOfMeasure);
    }

    [TestMethod]
    public void Get_AliasInPredefinedUnits_Properties()
    {
        //"degC", "C", "°C", "celsius", "degrees-celsius"
        var unit1 = Unit.Get("degC");
        var unit2 = Unit.Get("C");
        var unit3 = Unit.Get("°C");
        var unit4 = Unit.Get("celsius");
        var unit5 = Unit.Get("degrees-celsius");

        Assert.IsNotNull(unit1);
        Assert.IsNotNull(unit2);
        Assert.IsNotNull(unit3);
        Assert.IsNotNull(unit4);
        Assert.IsNotNull(unit5);

        Assert.AreEqual(unit1, unit2);
        Assert.AreEqual(unit1, unit3);
        Assert.AreEqual(unit1, unit4);
        Assert.AreEqual(unit1, unit5);
    }
}

[TestClass]
public class ValueVisitorTests
{
    [TestMethod]
    public void NumbersAndBoolsWorkInBooleanAndExpressions()
    {
        var env = new ParserEnvironment();
        env.AddVariable("fan run double", typeof(bool));
        env.AddVariable("boolean on", typeof(bool));

        var expr = Parser.Deserialize("[fan run double] && [boolean on]", env);

        var env2 = Env.Empty.Push();
        env2.Assign("fan run double", 1.0);
        env2.Assign("boolean on", true);

        var res = expr.EvaluateDirectUsingEnv(env2);

        res.HasValue.Should().BeTrue();
        res.Value.Should().Be(true);
    }

    [TestMethod]
    public void NumbersAndBoolsWorkInBooleanOrExpressions()
    {
        var env = new ParserEnvironment();
        env.AddVariable("fan run double", typeof(bool));
        env.AddVariable("boolean on", typeof(bool));

        var expr = Parser.Deserialize("[fan run double] || [boolean on]", env);

        var env2 = Env.Empty.Push();
        env2.Assign("fan run double", 0.0);
        env2.Assign("boolean on", false);

        var res = expr.EvaluateDirectUsingEnv(env2);

        res.HasValue.Should().BeTrue();
        res.Value.Should().Be(false);
    }
}
