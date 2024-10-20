using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abodit.Graph;
using Abodit.Mutable;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Services;
using WillowRules.Visitors;

namespace WillowExpressions.Test;

/// <summary>
/// Testing the new OPTION function (to be written in parser)
/// </summary>
[TestClass]
public class OptionsTests
{
	private static readonly Guid trendId = Guid.NewGuid();

	private readonly BasicDigitalTwinPoco twin = new BasicDigitalTwinPoco("mock") { trendID = trendId.ToString() };

	private IMemoryCache memoryCache = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

	private List<(string, TokenExpression)> list = new List<(string, TokenExpression)>();

	private Graph<ModelData, Relation> modelGraph = new Graph<ModelData, Relation>();

	private static ILogger<BindToTwinsVisitor> logger = Mock.Of<ILogger<BindToTwinsVisitor>>();

	[TestMethod]
	public void CanBindOptionsFunction()
	{
		var expr = Parser.Deserialize("OPTION([return air temperature], [AirTemperatureReturn;1]) < 27.5");
		var env = Env.Empty.Push().Assign("return air temperature", "<some guid>");
		var twinService = Moq.Mock.Of<ITwinService>();
		var twinSystemService = Moq.Mock.Of<ITwinSystemService>();
        var modelService = Moq.Mock.Of<IModelService>();
        var mlService = Moq.Mock.Of<IMLService>();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.Serialize().Should().Be("\"<some guid>\" < 27.5");
	}

	[TestMethod]
	public void CanParseOptionsFunction()
	{
		TokenExpression expr = Parser.Deserialize("OPTION([dtmi:com:willowinc:OccupancySensor;1], [occ sensor], [ChargiFi | Occupied]) > 0");
		expr.Should().BeOfType<TokenExpressionGreater>();
		TokenExpressionFunctionCall options = (TokenExpressionFunctionCall)((TokenExpressionGreater)expr).Left;
		TokenExpression[] parameters = options.Children;
		parameters[0].ToString().Should().Be("dtmi:com:willowinc:OccupancySensor;1");
		parameters[1].ToString().Should().Be("occ sensor");
		parameters[2].ToString().Should().Be("ChargiFi | Occupied");
		expr.Serialize().Should().Be("OPTION([dtmi:com:willowinc:OccupancySensor;1],[occ sensor],[ChargiFi | Occupied]) > 0");
	}

	[TestMethod]
	public void FailsBindingIfNotFound()
	{
		var expr = Parser.Deserialize("OPTION([return air temperature], [AirTemperatureReturn;1]) < 27.5");
		var env = Env.Empty.Push().Assign("air temperature", "<some guid>");

		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();
		var twinService = new Mock<ITwinService>();
		var modelService = Moq.Mock.Of<IModelService>();
		twinService.Setup(x => x.GetCachedTwin("mockid")).Returns(Task.FromResult<BasicDigitalTwinPoco?>(twin));
		var twinSystemService = new Mock<ITwinSystemService>();
		var mlService = Moq.Mock.Of<IMLService>();
		twinSystemService.Setup(x => x.GetTwinSystemGraph(It.IsAny<string[]>())).Returns(Task.FromResult(graph));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService.Object, twinSystemService.Object, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeFalse();
	}

	[TestMethod]
	[Ignore("Not using tags currently")]
	public void CanFuzzyBindOptionsFunction()
	{
		// Note: Fuzzy match not allowed at the moment, this is another strict match
		var expr = Parser.Deserialize("OPTION([vfd alarm sensor tamper], [vfd sensor tamper]) = 0");
		var env = Env.Empty.Push().Assign("vfd tamper alarm sensor", Parser.Deserialize("[some random guid]"));

		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();
		var twinService = new Mock<ITwinService>();
		var modelService = Moq.Mock.Of<IModelService>();
		twinService.Setup(x => x.GetCachedTwin(It.IsAny<string>())).Returns(Task.FromResult<BasicDigitalTwinPoco?>(twin));
		var twinSystemService = new Mock<ITwinSystemService>();
		var mlService = Moq.Mock.Of<IMLService>();
		twinSystemService.Setup(x => x.GetTwinSystemGraph(It.IsAny<string[]>())).Returns(Task.FromResult(graph));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService.Object, twinSystemService.Object, mlService, logger);
		var result = visitor.Visit(expr);
		result.Serialize().Should().Be("([some random guid] == 0)");
	}

	[TestMethod]
	public void CanFuzzyBindOptionsFunctionMoreComplex()
	{
		// Note: Fuzzy match not allowed at the moment, this is another strict match
		var expr1 = Parser.Deserialize("OPTION([vfdalarm sensor tamper], [vfd sensor tamper], [vfd alarm tamper], [vfd abc alarm tamper]) = 0");
		var env1 = Env.Empty.Push()
			.Assign("Tamper alarm sensor", Parser.Deserialize("[should not match this one]"))
			.Assign("vfd abc alarm tamper", Parser.Deserialize("[another random guid]"));

		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();
		var twinService = new Mock<ITwinService>();
		var modelService = Moq.Mock.Of<IModelService>();
		twinService.Setup(x => x.GetCachedTwin(It.IsAny<string>())).Returns(Task.FromResult<BasicDigitalTwinPoco?>(twin));
		var twinSystemService = new Mock<ITwinSystemService>();
		var mlService = Moq.Mock.Of<IMLService>();
		twinSystemService.Setup(x => x.GetTwinSystemGraph(It.IsAny<string[]>())).Returns(Task.FromResult(graph));

		var visitor1 = new BindToTwinsVisitor(env1, twin, memoryCache, modelService, twinService.Object, twinSystemService.Object, mlService, logger);
		var result1 = visitor1.Visit(expr1);
		result1.Serialize().Should().Be("[another random guid] == 0");
	}
}
