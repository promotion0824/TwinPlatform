using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Cache;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;
using Willow.Rules.Services;
using WillowRules.Test;
using WillowRules.Test.Bugs;
using WillowRules.Test.Bugs.Mocks;
using WillowRules.Visitors;

// ([this]!=0) & (CELSIUS([this])>-40) & (CELCIUS([this])<200)

namespace WillowExpressions.Test;

[TestClass]
public class BinginVisitorTests
{
	private static readonly Guid trendId = Guid.NewGuid();

	private static readonly BasicDigitalTwinPoco twin = new("mock")
	{
		Id = "mock",
		name = "HVAC V1",
		trendID = trendId.ToString(),
		Metadata = new DigitalTwinMetadataPoco()
		{
			ModelId = "dtmi:com:willowinc:OccupancyZone;1"
		},
		Contents = new()
		{
			["maxAirflowRating"] = 1930,
			["minAirflowRating"] = 520,
			["fan"] = new Dictionary<string, object>
			{
				["maxCurrent"] = 32
			}
		}
	};

	private static readonly BasicDigitalTwinPoco otherTwin = new("mock1")
	{
		Id = "mock1",
		name = "Terminal V1",
		Metadata = new DigitalTwinMetadataPoco()
		{
			ModelId = "dtmi:com:willowinc:TerminalUnit;1"
		}
	};

	private static readonly BasicDigitalTwinPoco eventTwin = new("mock2")
	{
		Id = "mock2",
		name = "Event V1",
		Contents = new(),
		Metadata = new DigitalTwinMetadataPoco()
		{
			ModelId = "dtmi:com:willowinc:Event;1"
		}
	};

	private static readonly BasicDigitalTwinPoco twinWithJobject = new("mock")
	{
		Id = "mock",
		name = "HVAC V1",
		trendID = trendId.ToString(),
		Contents = new()
		{
			["fan"] = new JObject()
			{
				["maxCurrent"] = 32
			}
		}
	};

	private static readonly BasicDigitalTwinPoco celcius = new("celcius")
	{
		trendID = Guid.NewGuid().ToString(),
		unit = "degC"
	};

	private static readonly BasicDigitalTwinPoco fahrenheit = new("fahrenheit")
	{
		trendID = Guid.NewGuid().ToString(),
		unit = "degF",
		Metadata = new DigitalTwinMetadataPoco()
		{
			ModelId = "dtmi:com:willowinc:Sensor;1"
		}
	};

	private static Lazy<ITwinService> twinServiceLazy = new(() =>
	{
		var mockTwinService = new Moq.Mock<ITwinService>();
		mockTwinService.Setup(x => x.GetCachedTwin(It.Is<string>(x => x == "mock"))).Returns(Task.FromResult<BasicDigitalTwinPoco?>(twin));
		mockTwinService.Setup(x => x.GetCachedTwin(It.Is<string>(x => x == "celsius"))).Returns(Task.FromResult<BasicDigitalTwinPoco?>(celcius));
		mockTwinService.Setup(x => x.GetCachedTwin(It.Is<string>(x => x == "fahrenheit"))).Returns(Task.FromResult<BasicDigitalTwinPoco?>(fahrenheit));
		return mockTwinService.Object;
	});

	private static ITwinService twinService = twinServiceLazy.Value;

	private static Lazy<ITwinSystemService> twinSystemServiceLazy = new Lazy<ITwinSystemService>(() =>
	{
		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();
		graph.AddStatement(fahrenheit, WillowRelation.isCapabilityOf, twin);
		graph.AddStatement(celcius, WillowRelation.isCapabilityOf, twin);
		var mockTwinSystemService = new Mock<ITwinSystemService>();
		mockTwinSystemService.Setup(x => x.GetTwinSystemGraph(It.IsAny<string[]>())).Returns(Task.FromResult(graph));
		return mockTwinSystemService.Object;
	});


	private static IMLService mlService = Moq.Mock.Of<IMLService>();

	private static Lazy<IModelService> modelServiceLazy = new(() =>
	{
		var dataCacheFactory = new DataCacheFactoryMock();

		var terminalUnitModel = new ModelData()
		{
			Id = "dtmi:com:willowinc:TerminalUnit;1",
			DtdlModel = new DtdlModel()
		};

		var twinModel = new ModelData()
		{
			Id = twin.Metadata.ModelId,
			DtdlModel = new DtdlModel()
			{
				extends = new StringList()
				{
					"dtmi:com:willowinc:TerminalUnit;1"
				}
			}
		};

		var models = new List<ModelData>()
		{
			terminalUnitModel,
			twinModel
		};

		dataCacheFactory.AllModelsCache.AddOrUpdate(MockObjects.WillowEnvironment.Id, "allmodels4", new CollectionWrapper<ModelData>(models)).Wait();

		var modelService = new ModelService(
			dataCacheFactory,
			Mock.Of<IADTService>(),
			MockObjects.WillowEnvironment,
			Mock.Of<ILogger<ModelService>>());

		return modelService!;
	});

	private static IModelService modelService = modelServiceLazy.Value;

	private static ITwinSystemService twinSystemService = Moq.Mock.Of<ITwinSystemService>();

	private static ILogger<BindToTwinsVisitor> logger = Mock.Of<ILogger<BindToTwinsVisitor>>();

	private IMemoryCache memoryCache = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

	[TestMethod]
	public void BindToJsonPropertyFromId()
	{
		var expr = Parser.Deserialize($"event.my_prop");

		var env = Env.Empty.Push();

		env.Assign("event", new TokenExpressionTwin(eventTwin));

		var mockTwinService = new Moq.Mock<ITwinService>();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, mockTwinService.Object, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		result.Serialize().Should().Be("([mock2]).my_prop");
	}

	[TestMethod]
	public void BindToJsonPropertyFromThis()
	{
		var expr = Parser.Deserialize($"this.my_prop");

		var env = Env.Empty.Push();

		var mockTwinService = new Moq.Mock<ITwinService>();

		var visitor = new BindToTwinsVisitor(env, eventTwin, memoryCache, modelService, mockTwinService.Object, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		result.Serialize().Should().Be("([mock2]).my_prop");
	}

	[TestMethod]
	public void ShouldNotPickupDuplicateTwinDueToMultipleRelationships()
	{
		var expr = Parser.Deserialize($"[{fahrenheit.Metadata.ModelId}]");
		var env = Env.Empty.Push();

		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();
		graph.AddStatement(twin, WillowRelation.isCapabilityOf, fahrenheit);
		graph.AddStatement(twin, WillowRelation.isContainedIn, fahrenheit);

		// a poorly configured model could have duplicate relationships going both ways
		graph.AddStatement(fahrenheit, WillowRelation.isCapabilityOf, twin);
		graph.AddStatement(fahrenheit, WillowRelation.isContainedIn, twin);

		var mockTwinService = new Mock<ITwinService>();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, mockTwinService.Object, twinSystemService, mlService, logger, rootGraph: graph);
		var result = visitor.Visit(expr);

		//instead of "{fahrenheit,fahrenheit}"
		result.ToString().Should().Be("fahrenheit");
	}

	[TestMethod]
	public async Task ShouldNotRewriteFailedExpression()
	{
		var parameters = new List<RuleParameter>()
		{
			new RuleParameter("Expression", "result", "IF(True," +
				"[dtmi:com:willowinc:ZoneAirTemperatureSensor;1] < 50," + // Truth
				"OPTION(ANY([dtmi:com:willowinc:MixedAirTemperatureSensor;1] < 50)))"), // Falsehood
		};

		var equipment = new TwinOverride("dtmi:com:willowinc:AirHandlingUnit;1", "NAU-88-CD-AHU");

		var rule = new Rule()
		{
			Id = "ahu-freeze-risk",
			PrimaryModelId = equipment.modelId,
			TemplateId = RuleTemplateAnyFault.ID,
			Parameters = parameters,
			Elements = [Fields.PercentageOfTime.With(0.90), Fields.OverHowManyHours.With(1), Fields.PercentageOfTimeOff.With(0.90)]
		};

		var sensors = new[]
		{
			// note: [dtmi:com:willowinc:ZoneAirTemperatureSensor;1] is missing
			new TwinOverride("dtmi:com:willowinc:MixedAirTemperatureSensor;1", "PNT6jnvtSvN9wvENGeX7WFr4z")
		};

		var harness = new ProcessorTestHarness();

		harness.OverrideCaches(rule, equipment, sensors);

		var result = await harness.GenerateRuleInstances();

		var boundParameter = result.First().RuleParametersBound.First(p => p.FieldId == "result");
		boundParameter.Status.Should().Be(RuleInstanceStatus.BindingFailed);
		boundParameter.PointExpression.ToString().Should().Be("IF(True, (FAILED('No twin matches found',dtmi:com:willowinc:ZoneAirTemperatureSensor;1) < 50), ANY((PNT6jnvtSvN9wvENGeX7WFr4z < 50)))");
	}

	[TestMethod]
	public void CanRewriteSimpleExpression()
	{
		var expr = Parser.Deserialize("!(a > b)");

		var env = Env.Empty.Push();
		env.Assign("a", 1234);
		env.Assign("b", 5678);

		var graph = new Abodit.Mutable.Graph<BasicDigitalTwinPoco, WillowRelation>();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		result.ToString().Should().Be("!(1234 > 5678)");
	}

	[TestMethod]
	public void CanRewriteSimpleExpressionFromLookup()
	{
		var expr = Parser.Deserialize("!(mock > b)");

		var env = Env.Empty.Push();
		env.Assign("b", 5678);

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = (TokenExpressionNot)visitor.Visit(expr);

		var greaterExpression = (TokenExpressionGreater)result.Child;

		var twinExpression = ((TokenExpressionTwin)greaterExpression.Left).Value;

		result.ToString().Should().Be($"!(mock > 5678)");
		twinExpression.Should().Be(twin);

		env.Variables.Should().HaveCount(1);
		env.Variables.First().Should().Be("b");
	}

	[TestMethod]
	public void CanRewriteCelsiusExpression()
	{
		var expr = Parser.Deserialize("([sat]!=0) & (CELSIUS([sat])>-40) & (CELCIUS([sat])<200)");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degF" });

		var visitor = new BindToTwinsVisitor(env, fahrenheit, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("(((GUID != 0) & (((GUID - 32) * 0.555555555555556) > -40)) & (((GUID - 32) * 0.555555555555556) < 200))");
	}

	[TestMethod]
	public void CanRewriteDegreesCelsiusExpression()
	{
		var expr = Parser.Deserialize("([sat]!=0) & (CELSIUS([sat])>-40) & (CELCIUS([sat])<200)");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("(((GUID != 0) & (GUID > -40)) & (GUID < 200))");
	}

	[TestMethod]
	public void CanRewriteDegreesCelsiusExpressionToFarenheit()
	{
		var expr = Parser.Deserialize("([rat]!=0) & (FAHRENHEIT([rat])>40) & (FAHRENHEIT([rat])<90)");

		var env = Env.Empty.Push();
		env.Assign("rat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("(((GUID != 0) & ((32 + (GUID * 1.8)) > 40)) & ((32 + (GUID * 1.8)) < 90))");
	}

	[TestMethod]
	public void CanRewriteCfmToLps()
	{
		var expr = Parser.Deserialize("METRIC([af]) > 40");

		var env = Env.Empty.Push();
		env.Assign("af", new TokenExpressionVariableAccess("airflow") { Unit = Unit.cfm.Name });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("((airflow * 0.47194745) > 40)");
	}

	[TestMethod]
	public void CanRewriteLpsToLps()
	{
		var expr = Parser.Deserialize("METRIC([af]) > 40");

		var env = Env.Empty.Push();
		env.Assign("af", new TokenExpressionVariableAccess("airflow") { Unit = Unit.lps.Name });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("(airflow > 40)");
	}

	[TestMethod]
	public void CannotRewritePercentageToLps()
	{
		var expr = Parser.Deserialize("METRIC([afs]) > 40");

		var env = Env.Empty.Push();
		env.Assign("afs", new TokenExpressionVariableAccess("[airflow setpoint]") { Unit = Unit.percentage.Name });

		var twinService = Moq.Mock.Of<ITwinService>();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeFalse();
		result.ToString().Should().Be("(FAILED('Cannot coerce % to METRIC',METRIC([airflow setpoint])) > 40)");
	}

	[TestMethod]
	public void PercentageFunction()
	{
		var expr = Parser.Deserialize("PERCENTAGE([air_flow]) > 0.1");

		var env = Env.Empty.Push();
		env.Assign("air_flow", new TokenExpressionVariableAccess("[airflow setpoint]") { Unit = Unit.percentage.Name });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("([airflow setpoint] > 0.1)");
	}

	[TestMethod]
	public void PercentageFunctionWithPercent100()
	{
		var expr = Parser.Deserialize("PERCENTAGE([air_flow]) > 0.1");

		var env = Env.Empty.Push();
		env.Assign("air_flow", new TokenExpressionVariableAccess("[airflow setpoint]") { Unit = Unit.percentage100.Name });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("(([airflow setpoint] * 0.01) > 0.1)");
	}

	[TestMethod]
	public void PercentageFunctionWithFailure()
	{
		var expr = Parser.Deserialize("PERCENTAGE([afs]) > 0.1");

		var env = Env.Empty.Push();
		env.Assign("afs", new TokenExpressionVariableAccess("[airflow setpoint]") { Unit = Unit.cfm.Name });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeFalse();
		result.ToString().Should().Be("(FAILED('Cannot coerce % to METRIC',PERCENTAGE([airflow setpoint])) > 0.1)");
	}

	[TestMethod]
	public void IfPartFailsAllFails()
	{
		var expr = Parser.Deserialize("a-b");

		var env = Env.Empty.Push();
		env.Assign("a", Parser.Deserialize("FAILED([not recognized tags])"));
		env.Assign("b", Parser.Deserialize("8765-1234"));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		Console.WriteLine(result.Serialize());

		visitor.Success.Should().BeFalse("a failing child expression should fail subtract");

		result.ToString().Should().Be("(FAILED(not recognized tags) - (8765 - 1234))");
	}

	[TestMethod]
	public void IfPartFailsOptionCanStillWork()
	{
		var list = new List<(string var, TokenExpression tokenExpression)>();
		var expr = Parser.Deserialize("OPTION(a-b, c)");

		var env = Env.Empty.Push();
		env.Assign("a", Parser.Deserialize("FAILED([not recognized tags])"));
		env.Assign("b", Parser.Deserialize("8765-4321"));
		env.Assign("c", Parser.Deserialize("[delta_sensor]"));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		visitor.Success.Should().BeTrue("Option can work around an indirect failure");
		result.ToString().Should().Be("delta_sensor");
	}

	[TestMethod]
	public void TolerantOptionShouldRemoveFailedBindings()
	{
		var expr = Parser.Deserialize("TOLERANTOPTION(a, b, c)");

		var env = Env.Empty.Push();
		env.Assign("b", Parser.Deserialize("2"));
		env.Assign("c", Parser.Deserialize("3 + 1"));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		
		result.ToString().Should().Be("TOLERANTOPTION(2, (3 + 1))");
		visitor.Success.Should().BeTrue();
	}

	[TestMethod]
	public void TolerantOptionShouldRemoveOnlyKeepFirstConst()
	{
		var expr = Parser.Deserialize("TOLERANTOPTION(a, b, c)");

		var env = Env.Empty.Push();
		env.Assign("b", Parser.Deserialize("2"));
		env.Assign("c", Parser.Deserialize("3"));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		result.ToString().Should().Be("2");
		visitor.Success.Should().BeTrue();
	}

	[TestMethod]
	public void TolerantOptionShouldFailBindings()
	{
		var expr = Parser.Deserialize("TOLERANTOPTION(a, b)");

		var env = Env.Empty.Push();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);

		result.ToString().Should().Be("TOLERANTOPTION(FAILED('Could not resolve variable',a), FAILED('Could not resolve variable',b))");
		visitor.Success.Should().BeFalse();
	}

	[TestMethod]
	public void CanBindToProperty()
	{
		var expr = Parser.Deserialize("this.maxAirflowRating");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("1930");
	}

	[TestMethod]
	public void TemporalExpressionMustBindToTwinId()
	{
		var expr = Parser.Deserialize("MIN(this, 1h)");

		var env = Env.Empty.Push();
		//env.Assign("dtmi:com:willowinc:OccupancyZone;1", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.Serialize().Should().Be("MIN([mock], 1[h])");
	}

	[TestMethod]
	public void TemporalExpressionMustBindToVariableName()
	{
		var expr = Parser.Deserialize("MIN(a, 1h, -1h)");

		var env = Env.Empty.Push();
		env.Assign("a", Parser.Deserialize("1 + 1"));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.Serialize().Should().Be("MIN(a, 1[h], (-1[h]))");
	}

	[TestMethod]
	public void CanEvaluateModelIdBasedOnInheritedModelTrue()
	{
		var expr = Parser.Deserialize("IF(this is [dtmi:com:willowinc:TerminalUnit;1], 1, 2)");

		var env = Env.Empty.Push();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("IF(True, 1, 2)");
	}

	[TestMethod]
	public void CanEvaluateModelIdTrue()
	{
		var expr = Parser.Deserialize("IF(this is [dtmi:com:willowinc:OccupancyZone;1], 1, 2)");

		var env = Env.Empty.Push();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("IF(True, 1, 2)");
	}

	[TestMethod]
	public void CanEvaluateModelIdFalse()
	{
		var expr = Parser.Deserialize("IF(this is [dtmi:com:willowinc:OtherModel;1], 1, 2)");

		var env = Env.Empty.Push();

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("IF(False, 1, 2)");
	}

	[TestMethod]
	public void CanEvaluateModelIdArray()
	{
		var expr = Parser.Deserialize("IF({this, this} is [dtmi:com:willowinc:OtherModel;1], 1, 2)");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("{IF(False, 1, 2),IF(False, 1, 2)}");
	}

	[TestMethod]
	public void CanEvaluateModelIdFolded()
	{
		var expr = Parser.Deserialize("IF({this is [dtmi:com:willowinc:OtherModel;1], this is [dtmi:com:willowinc:OccupancyZone;1]}, 1, 2)");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("{IF(False, 1, 2),IF(True, 1, 2)}");
	}

	[TestMethod]
	public void CanBindToPropertyOfProperty()
	{
		var expr = Parser.Deserialize("this.fan.maxCurrent");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("32");
	}

	[TestMethod]
	public void CanBindToPropertyOfPropertyUsingJObject()
	{
		var expr = Parser.Deserialize("this.fan.maxCurrent");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twinWithJobject, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeTrue();
		result.ToString().Should().Be("32");
	}

	[TestMethod]
	public void CanFailBindToPropertyOfProperty()
	{
		var expr = Parser.Deserialize("this.fan.maxCurrentMissing");

		var env = Env.Empty.Push();
		env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeFalse();
		result.ToString().Should().Be("FAILED('Could not resolve property',this.fan.maxCurrentMissing)");
	}

	[TestMethod]
	public void CanFailBindHigherToPropertyOfProperty()
	{
		var expr = Parser.Deserialize("this.fanMissing.maxCurrent");

		var env = Env.Empty.Push();
		//env.Assign("this", new TokenExpressionTwin(twin));

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		visitor.Success.Should().BeFalse();
		result.ToString().Should().Be("FAILED('Missing property for twin mock',this.fanMissing)");
	}

	[TestMethod]
	public void ScanModelIdsPropertyAccess()
	{
		var expr = Parser.Deserialize("[dtmi:com:willowinc:OccupancyZone;1].[dtmi:com:willowinc:PeopleCountSensor;1]");
		var visitor = new ModelIdScannerVisitor();
		var result = visitor.Visit(expr);
		visitor.ModelIds.Count.Should().Be(2);
		visitor.ModelIds.Contains("dtmi:com:willowinc:PeopleCountSensor;1").Should().BeTrue();
		visitor.ModelIds.Contains("dtmi:com:willowinc:OccupancyZone;1").Should().BeTrue();
	}

	[TestMethod]
	public void ScanModelIdsFunction()
	{
		var expr = Parser.Deserialize("OPTION([dtmi:com:willowinc:OccupancyZone;1],[dtmi:com:willowinc:PeopleCountSensor;1])");
		var visitor = new ModelIdScannerVisitor();
		var result = visitor.Visit(expr);
		visitor.ModelIds.Count.Should().Be(2);
		visitor.ModelIds.Contains("dtmi:com:willowinc:PeopleCountSensor;1").Should().BeTrue();
		visitor.ModelIds.Contains("dtmi:com:willowinc:OccupancyZone;1").Should().BeTrue();
	}

	[TestMethod]
	public void CanFoldFunctionCallIntoArray()
	{
		var expr = Parser.Deserialize("SIN({2, 3, 4, 5, IF([sat]!=0, 1, 0)})");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("{SIN(2),SIN(3),SIN(4),SIN(5),SIN(IF((FAILED('Could not resolve variable',GUID) != 0), 1, 0))}");
	}

	[TestMethod]
	public void FoldingIntoSelfReferencingVariableShouldFail()
	{
		var expr = Parser.Deserialize("zat + {1, 2}");

		var env = Env.Empty.Push();

		var ignored = new string[] { "zat" };

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger, ignoredIdentifiers: ignored);

		var result = visitor.Visit(expr);

		env.Assign("zat", result);

		expr = Parser.Deserialize("zat > 1");
		visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		result = visitor.Visit(expr);
		result.ToString().Should().Be("{FAILED('Max array count of 10 reached',{FAILED('Max array count of 10 reached',{FAILED('Max array count of 10 reached',{FAILED('Max array count of 10 reached',{(FAILED('Max array count of 10 reached',{(1 + zat)}) + 1)})})})}),(FAILED('Max array count of 10 reached',{(1 + zat)}) > 1)}");
	}

	[TestMethod]
	public void TemporalShouldFailMaxArrayCount()
	{
		var expr = Parser.Deserialize("{var1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}");

		var env = Env.Empty.Push();

		env.Assign("zat", expr);

		expr = Parser.Deserialize("COUNT(zat, 20d) > 10");
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("(FAILED('Max array count of 10 reached',{var1}) > 10)");
	}

	[TestMethod]
	public void CanFoldTernaryIfIntoArray()
	{
		var expr = Parser.Deserialize("IF({2, 3, 4, 5, [sat]!=0}, 1, 0)");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be("{IF(2, 1, 0),IF(3, 1, 0),IF(4, 1, 0),IF(5, 1, 0),IF((FAILED('Could not resolve variable',GUID) != 0), 1, 0)}");
	}

	[TestMethod]
	public void TwinFilterByNameMustBeTrue()
	{
		var expr = Parser.Deserialize("contains(this.name, 'V1')");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		var result = resultExpression.EvaluateDirectUsingEnv(env);
		result.Value.ToBoolean(null).Should().BeTrue();
	}

	[TestMethod]
	public void TwinFilterByPathBeTrue()
	{
		var expr = Parser.Deserialize("contains(this.fan.maxCurrent, '3')");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		var result = resultExpression.EvaluateDirectUsingEnv(env);
		result.Value.ToBoolean(null).Should().BeTrue();
	}

	[TestMethod]
	public void TwinFilterByNameMustBeFalse()
	{
		var expr = Parser.Deserialize("startswith(this.name, 'V1')");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		var result = resultExpression.EvaluateDirectUsingEnv(env);
		result.Value.ToBoolean(null).Should().BeFalse();
	}

	[TestMethod]
	public void AnyShouldEvaluateToTrue()
	{
		var expr = Parser.Deserialize("ANY({0, 1, 0, 1})");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		var result = resultExpression.EvaluateDirectUsingEnv(env);
		result.Value.ToBoolean(null).Should().BeTrue();
	}

	[TestMethod]
	public void AllShouldEvaluateToFalse()
	{
		var expr = Parser.Deserialize("ANY({1, 0, 1})");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		var result = resultExpression.EvaluateDirectUsingEnv(env);
		result.Value.ToBoolean(null).Should().BeTrue();
	}

	[TestMethod]
	[DataRow(">")]
	[DataRow("<")]
	[DataRow(">=")]
	[DataRow("<=")]
	[DataRow("=")]
	[DataRow("!=")]
	public void CanFoldComparisonIntoArray(string op)
	{
		var expr = Parser.Deserialize($"{{2, 3, 4, sat}} {op} 0");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be($"{{(2 {op} 0),(3 {op} 0),(4 {op} 0),(FAILED('Could not resolve variable',GUID) {op} 0)}}");
	}

	[TestMethod]
	[DataRow(">")]
	[DataRow("<")]
	[DataRow(">=")]
	[DataRow("<=")]
	[DataRow("=")]
	[DataRow("!=")]
	public void CanFoldComparisonIntoArrayReversed(string op)
	{
		var expr = Parser.Deserialize($"0 {op} {{2, 3, 4, sat}}");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be($"{{(0 {op} 2),(0 {op} 3),(0 {op} 4),(0 {op} FAILED('Could not resolve variable',GUID))}}");
	}

	[TestMethod]
	[DataRow("+")]
	[DataRow("-")]
	[DataRow("*")]
	[DataRow("/")]
	public void CanFoldMathOperatorsIntoArray(string op)
	{
		var expr = Parser.Deserialize($"{{2, 3, 4, sat}} {op} 0");

		var env = Env.Empty.Push();
		env.Assign("sat", new TokenExpressionVariableAccess("GUID") { Unit = "degrees-celsius" });

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(expr);
		result.ToString().Should().Be($"{{(2 {op} 0),(3 {op} 0),(4 {op} 0),(FAILED('Could not resolve variable',GUID) {op} 0)}}");
	}

	[TestMethod]
	public void CanFoldFromEnvironment()
	{
		var arrayExp = Parser.Deserialize($"{{1, 2, 3, 4}}") as TokenExpressionArray;
		arrayExp.Should().NotBeNull();

		var exp = Parser.Deserialize($"a + 9");

		var env = Env.Empty.Push();
		env.Assign("a", arrayExp!);

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var result = visitor.Visit(exp);
		result.ToString().Should().Be($"{{(1 + 9),(2 + 9),(3 + 9),(4 + 9)}}");
	}

	[TestMethod]
	public void AnySimplifiesThree()
	{
		var expr = Parser.Deserialize("ANY({true, true, false})");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("True | True | False");
	}

	[TestMethod]
	public void AllSimplifiesThree()
	{
		var expr = Parser.Deserialize("ALL({true, true, false})");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("True & True & False");
	}

	[TestMethod]
	public void EachParses()
	{
		var expr = Parser.Deserialize("EACH({true, true, false}, item, item)");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("{True,True,False}");
	}

	[TestMethod]
	public void EachFailsForEmptyResult()
	{
		var expr = Parser.Deserialize("MAX(EACH(myvar, item, item.notfound))");
		var env = Env.Empty.Push();
		env.Assign("myvar", new TokenExpressionTwin(new BasicDigitalTwinPoco("invalid")
		{
			Metadata = new DigitalTwinMetadataPoco()
			{
				ModelId = "id"
			},
			Contents = new Dictionary<string, object>()
		}));
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("MAX(FAILED(\"EACH Argument is empty. 1 failed.\",{FAILED(\"Missing property for twin invalid\",item.notfound)}))");
	}

	[TestMethod]
	public void EachWorksOnASingleItem()
	{
		var expr = Parser.Deserialize("EACH(true, item, item)");
		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("True");
	}

	[TestMethod]
	public void GlobalMacroFromFunction()
	{
		var body = Parser.Deserialize("p1 + 5");
		RegisteredFunctionArgument arg = new("p1", typeof(object));

		RegisteredFunction registeredFunction = RegisteredFunction.Create("macro1", new RegisteredFunctionArgument[] { arg }, body);

		var expr = Parser.Deserialize("macro1(10)");
		var env = Env.Empty.Push();

		env.Assign(registeredFunction.Name, registeredFunction);

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("10 + 5");
	}

	[TestMethod]
	public void GlobalMacroFromVariable()
	{
		var body = Parser.Deserialize("10 + 5");

		RegisteredFunction registeredFunction = RegisteredFunction.Create("macro1", new RegisteredFunctionArgument[0], body);

		var expr = Parser.Deserialize("macro1");
		var env = Env.Empty.Push();

		env.Assign(registeredFunction.Name, registeredFunction);

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("10 + 5");
	}

	[TestMethod]
	public void FindAllUsingADTQuery()
	{
		var body = Parser.Deserialize("10 + 5");

		RegisteredFunction registeredFunction = RegisteredFunction.Create("macro1", new RegisteredFunctionArgument[0], body);

		var expr = Parser.Deserialize("macro1");
		var env = Env.Empty.Push();

		env.Assign(registeredFunction.Name, registeredFunction);

		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("10 + 5");
	}

	[TestMethod]
	public void EachWorksOnTwinProperties()
	{
		var twins = Enumerable.Range(1, 3).Select(x =>
			new TokenExpressionTwin(new BasicDigitalTwinPoco($"ID-{x}")
			{
				Contents = new Dictionary<string, object>
				{
					["index"] = x,
					["value"] = 10 - x
				}
			})
			);

		var body = Parser.Deserialize("item.index * item.value");

		var expr = new TokenExpressionEach(
			new TokenExpressionArray(twins.ToArray()),
			new TokenExpressionVariableAccess("item"),
			body);

		var env = Env.Empty.Push();
		var visitor = new BindToTwinsVisitor(env, twin, memoryCache, modelService, twinService, twinSystemService, mlService, logger);
		var resultExpression = visitor.Visit(expr);
		string output = resultExpression.Serialize();
		output.Should().Be("{1 * 9,2 * 8,3 * 7}");

		Console.Write(expr.Serialize());
		Console.Write(" => ");
		Console.WriteLine(output);
	}
}
