using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Willow.ExpressionParser;
using Willow.Expressions;
using Willow.Expressions.Visitor;
using Willow.Rules.Model;

namespace Willow.Rules.Test;

[TestClass]
public class FilterVisitorTests
{
	[TestMethod]
	public void CanHandleTwinId()
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize("[MS-PS-B122]");
		var result = visitor.Visit(expression);
		result.Status.Should().Be(FilterResultType.ServerSide);
		string debug = result.AdtQuery;
		debug.Should().Be("SELECT * FROM DIGITALTWINS twin WHERE twin.$dtId == 'MS-PS-B122'");
	}

	[TestMethod]
	public void CanFAllBackToTwinIdDelegate()
	{
		var visitor = new TwinFilterVisitor(getIdsFromExpressions: (e) => ["my_id"]);

		var expression = Parser.Deserialize("UNDER([dtmi:Room;1])");
		var result = visitor.Visit(expression);
		result.Status.Should().Be(FilterResultType.ServerSide);
		string debug = result.AdtQuery;
		debug.Should().Be("SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'my_id'");
	}

	[TestMethod]
	public void CanHandleModelId()
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize("[dtmi:com:willowinc:Building;1]");
		var result = visitor.Visit(expression);
		result.Status.Should().Be(FilterResultType.ServerSide);
		string debug = result.AdtQuery;
		debug.Should().Be("SELECT * FROM DIGITALTWINS twin WHERE IS_OF_MODEL(twin,'dtmi:com:willowinc:Building;1')");
		// OR debug.Should().Be("SELECT * FROM DIGITALTWINS DT WHERE IS_OF_MODEL('dtmi:com:willowinc:Building;1')");
	}

	// AND T.$dtId in ['123', '456']

	[TestMethod]
	public void CanHandleOrClauseTwinId()
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize("[MS-PS-B122] | [MS-PS-B121]");
		var result = visitor.Visit(expression);
		string debug = result.AdtQuery;
		result.Status.Should().Be(FilterResultType.ServerSide);
		debug.Should().Be("SELECT * FROM DIGITALTWINS twin WHERE twin.$dtId == 'MS-PS-B122' | twin.$dtId == 'MS-PS-B121'");
	}

	[TestMethod]
	public void CanHandleTwinChild()
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize("[MS-PS-B122].[dtmi:com:willow:inc:ZoneTemperature;1]");
		var result = visitor.Visit(expression);
		result.Status.Should().Be(FilterResultType.ServerSide);
		result.AdtQuery.Should().Be("SELECT TOP (1001) child,twin FROM DIGITALTWINS MATCH (child)<-[:isCapabilityOf]-(twin) WHERE twin.$dtId == 'MS-PS-B122'");
	}

	public void CanHandleModelChild()
	{
		// [dtmi:com:willowinc:HVACEquipment;1]
	}

	[TestMethod]
	public void CanHandleAndOrClauseTwinId()
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize("([MS-PS-B122] | [MS-PS-B121]) & ![dtmi:com:willowinc:HVACEquipment;1]");
		var result = visitor.Visit(expression);
		string debug = result.AdtQuery;
		result.Status.Should().Be(FilterResultType.ServerSide);
		debug.Should().Be("SELECT * FROM DIGITALTWINS twin WHERE (twin.$dtId == 'MS-PS-B122' | twin.$dtId == 'MS-PS-B121') & !IS_OF_MODEL(twin,'dtmi:com:willowinc:HVACEquipment;1')");
	}

	[TestMethod]
	public void CanHandleClientAndOrClauseTwinId()
	{
		var visitor = new TwinFilterVisitor(clientSideVariableName: "DT");
		var expression = Parser.Deserialize("(ABS(DT.width) > 2 | DT.width == 1) & (([MS-PS-B122] | [MS-PS-B121]) & ![dtmi:com:willowinc:HVACEquipment;1])");
		var result = visitor.Visit(expression);
		string debug = result.AdtQuery;

		debug.Should().Be("SELECT * FROM DIGITALTWINS twin WHERE ((twin.$dtId == 'MS-PS-B122' | twin.$dtId == 'MS-PS-B121') & !IS_OF_MODEL(twin,'dtmi:com:willowinc:HVACEquipment;1'))");

		string clientDebug = $"{result.Client.Serialize()}";

		clientDebug.Should().Be("(ABS(DT.width) > 2 | DT.width == 1)");

		Console.WriteLine();
		Console.WriteLine(expression);
		Console.WriteLine("becomes");
		Console.WriteLine(debug);
		Console.WriteLine("and then on the client side we do a filter");
		Console.WriteLine(clientDebug);

		string exprDebug = $"{result.Expression.Serialize()}";
		Console.WriteLine("OR we can create a single client-side expression to evaluate 403 access");
		Console.WriteLine(exprDebug);

		//var exprConversion =  new Willow.Expressions.Visitor.ConvertToExpressionVisitor<BasicDigitalTwinPoco>(x => x.NodeType);

		result.Status.Should().Be(FilterResultType.Forked);
	}

	[TestMethod]
	[DataRow("UNDER([MS-PS-B122])",
		"SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'MS-PS-B122'")]
	[DataRow("UNDER([MS-PS-B122]) | UNDER([MS-PS-B121])",
		"SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId IN ['MS-PS-B122', 'MS-PS-B121']")]
	public void CanHandleFilter(string input, string expected)
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize(input);
		var result = visitor.Visit(expression);
		string debug = result.AdtQuery;
		result.Status.Should().Be(FilterResultType.ServerSide);
		debug.Should().Be(expected);
	}

	[TestMethod]
	[DataRow("UNDER([MS-PS-B122]) & [dtmi:com:willowinc:HVACEquipment;1]",
		"SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'MS-PS-B122' AND IS_OF_MODEL(twin,'dtmi:com:willowinc:HVACEquipment;1')")]
	[DataRow("UNDER([MS-PS-B122]) & ([dtmi:com:willowinc:HVACEquipment;1] | [dtmi:com:willowinc:Space;1])",
		"SELECT TOP (1001) twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn|locatedIn|isCapabilityOf|includedIn*..5]->(ancestor) WHERE ancestor.$dtId = 'MS-PS-B122' AND (IS_OF_MODEL(twin,'dtmi:com:willowinc:HVACEquipment;1') | IS_OF_MODEL(twin,'dtmi:com:willowinc:Space;1'))")]
	//  "SELECT twin,ancestor FROM DIGITALTWINS MATCH (twin)-[:isPartOf|isContainedIn*..5]->(ancestor) WHERE ancestor = 'MS-PS-B122' AND (IS_OF_MODEL(twin,"dtmi:com:willowinc:HVACEquipment;1") & IS_OF_MODEL(twin,"dtmi:com:willowinc:Space;1"))"
	public void CanHandleFilterModelAndUnder(string input, string expected)
	{
		var visitor = new TwinFilterVisitor();
		var expression = Parser.Deserialize(input);
		var result = visitor.Visit(expression);
		string debug = result.AdtQuery;
		result.Status.Should().Be(FilterResultType.ServerSide);
		debug.Should().Be(expected);
	}


	public class VisitorContext
	{
		public string Id { get; }

		public bool UNDER(string twinId)
		{
			return true;
		}

		public VisitorContext(string id)
		{
			this.Id = id;
		}

		public static Expression GetterForVariableName(Expression instance, string variableName)
		{
			// TODO: Use this and the variable name to call a method on self (!)
			return Expression.Constant(new VisitorContext(variableName + " call " + instance.ToString()));
		}

		public override string ToString() => $"Context: {this.Id}";
	}

	[TestMethod]
	public void TestAgainstRuleProperty()
	{
		Expression<Func<string, string>> getter = (string x) => x;

		var rule = new Rule("test", "id", "id");

		var visitor = new ConvertToExpressionVisitor<Rule>((e, s) =>
		{
			return e;
		});

		var expression = Parser.Deserialize("this.Name == 'test'");

		var result = (LambdaExpression)visitor.Visit(expression);

		var myDelegate = (Func<Rule, bool>)result.Compile();

		Expression<Func<Rule, bool>> whereExpression1 = (Rule r) => r.Name == "test";

		Expression<Func<Rule, bool>> whereExpression = (Expression<Func<Rule, bool>>)result;

		var rules = new List<Rule>()
		{
			new Rule("not test", "id", "id"),
			new Rule("test", "id", "id")
		};

		var filtered = rules.Where(myDelegate).ToList();

		filtered.Count.Should().Be(1);
		filtered[0].Should().Be(rules[1]);
	}

	[TestMethod]
	[Ignore("Still under development")]
	public void CanHandleUnderOrUnderFilterToDotnetExpression()
	{
		Expression<Func<string, string>> getter = (string x) => x;

		var visitor = new ConvertToExpressionVisitor<VisitorContext>(VisitorContext.GetterForVariableName);

		visitor.AddTwoParameterBoolFunction("UNDER", typeof(VisitorContext).GetMethod("UNDER")!);

		// TODO: Allow registration of external functions in the visitor instead of on the class passed

		var expression = Parser.Deserialize("UNDER([MS-PS-B122]) | UNDER([MS-PS-B121])");
		var result = visitor.Visit(expression);

		string debug = result.ToString();
		Console.WriteLine();
		Console.WriteLine(expression);
		Console.WriteLine("becomes");
		Console.WriteLine(debug);

		debug.Should().Be("source => (Context: MS-PS-B122 call source.UNDER() OrElse Context: MS-PS-B121 call source.UNDER())");
	}
}
