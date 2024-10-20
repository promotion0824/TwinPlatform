using FluentAssertions;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Willow.Expressions;
using Willow.Rules.Model;
using Willow.Rules.Model.RuleTemplates;

namespace WillowRules.Test.Models;

[TestClass]
public class ActorStateTests
{
	[DataTestMethod]
	[DataRow(0, 0)]
	[DataRow(0, 10)]
	[DataRow(0, 11)]
	[DataRow(0, 9)]
	[DataRow(1, 10)]
	[DataRow(1, 11)]
	[DataRow(1, 9)]
	[DataRow(-1, 10)]
	[DataRow(-1, 11)]
	[DataRow(-1, 9)]
	[DataRow(-1, 0)]
	[DataRow(-10, -9)]
	public void ShouldRemovePreviousOutput(double start, int end)
	{
		var outputValues = new OutputValues();

		var startTime = DateTimeOffset.Now.AddMinutes(-1);

		var v1 = new OutputValue(startTime, startTime.AddSeconds(10), false, false, "", "", new KeyValuePair<string, object>[0]);

		outputValues.Add(v1);

		var v2 = new OutputValue(startTime.AddSeconds(start), startTime.AddSeconds(end), false, false, "", "", new KeyValuePair<string, object>[0]);

		outputValues.Add(v2);
		outputValues.Count.Should().Be(1);
		outputValues.Points[0].Should().BeEquivalentTo(v2);
		outputValues.IsInOrder().Should().BeTrue();
	}

	[DataTestMethod]
	[DataRow(0, 0)]
	[DataRow(0, 10)]
	[DataRow(0, 11)]
	[DataRow(0, 9)]
	[DataRow(1, 10)]
	[DataRow(1, 11)]
	[DataRow(1, 9)]
	[DataRow(-1, 10)]
	[DataRow(-1, 11)]
	[DataRow(-1, 9)]
	[DataRow(-1, 0)]
	[DataRow(-10, -9)]
	public void ShouldRemovePreviousCommand(double start, int end)
	{
		var outputValues = new OutputValuesCommand();

		var startTime = DateTimeOffset.Now.AddMinutes(-1);

		var v1 = new CommandOutputValue(startTime, startTime.AddSeconds(10), startTime, null, false, 1);

		outputValues.Add(v1);

		var v2 = new CommandOutputValue(startTime.AddSeconds(start), startTime.AddSeconds(end), startTime.AddSeconds(start), null, false, 1);

		outputValues.Add(v2);
		outputValues.Count.Should().Be(1);
		outputValues.Points[0].StartTime.Should().BeExactly(v2.StartTime);
		outputValues.Points[0].EndTime.Should().BeExactly(v2.EndTime);
		outputValues.IsInOrder().Should().BeTrue();
	}

	[TestMethod]
	public void ShouldSquashUntriggeredCommandOutput()
	{
		var outputValues = new OutputValuesCommand();

		var startTime = DateTimeOffset.Now;

		outputValues.WithOutput(startTime, startTime, startTime, false, 0);

		outputValues.Points[0].TriggerStartTime.Should().BeExactly(startTime);
		outputValues.Points[0].TriggerEndTime.Should().BeExactly(startTime);

		outputValues.WithOutput(startTime.AddMinutes(1), startTime.AddMinutes(1), startTime.AddMinutes(1), false, 0);

		outputValues.Count.Should().Be(1);
		outputValues.Points[0].EndTime.Should().Be(startTime.AddMinutes(1));
		//the trigger start time should not change if an untriggered output is overwritten by another untriggerd
		outputValues.Points[0].TriggerStartTime.Should().BeExactly(startTime);
		outputValues.Points[0].TriggerEndTime.Should().BeExactly(startTime);

		outputValues.WithOutput(startTime.AddMinutes(2), startTime.AddMinutes(2), null, true, 0);

		outputValues.Points.Count.Should().Be(2);

		outputValues.Points[1].TriggerStartTime.Should().BeExactly(startTime.AddMinutes(2));
		outputValues.Points[1].TriggerEndTime.Should().BeNull();

		outputValues.WithOutput(startTime.AddMinutes(3), startTime.AddMinutes(3), startTime.AddMinutes(3), false, 0);

		outputValues.Points.Count.Should().Be(3);

		outputValues.Points[2].TriggerStartTime.Should().BeExactly(startTime.AddMinutes(3));
		outputValues.Points[2].TriggerEndTime.Should().BeExactly(startTime.AddMinutes(3));
	}

	[DataTestMethod]
	[DataRow(10, 12)]
	[DataRow(11, 12)]
	public void ShouldNotRemovePreviousOutput(double start, int end)
	{
		var outputValues = new OutputValues();

		var startTime = DateTimeOffset.Now.AddMinutes(-1);

		var v1 = new OutputValue(startTime, startTime.AddSeconds(10), false, false,  "", "", new KeyValuePair<string, object>[0]);

		outputValues.Add(v1);

		var v2 = new OutputValue(startTime.AddSeconds(start), startTime.AddSeconds(end), false, false, "", "", new KeyValuePair<string, object>[0]);

		outputValues.Add(v2);
		outputValues.Count.Should().Be(2);
		outputValues.IsInOrder().Should().BeTrue();
	}

	[TestMethod]
	public void MustSquashSameCategory()
	{
		var startTime = DateTimeOffset.Now.AddMinutes(-1);

		var actor = new ActorState("rule", "ri", startTime, 1);

		var v1 = new OutputValue(startTime, startTime.AddSeconds(10), false, false, "invalid", "", new KeyValuePair<string, object>[0]);

		actor.OutputValues.Add(v1);

		actor.MissingValue(startTime.AddSeconds(20));

		actor.OutputValues.Count.Should().Be(2);
		actor.OutputValues.Points.Last().EndTime.Should().Be(startTime.AddSeconds(20));

		actor.MissingValue(startTime.AddSeconds(30));

		actor.OutputValues.Count.Should().Be(2);
		actor.OutputValues.Points.Last().EndTime.Should().Be(startTime.AddSeconds(30));
	}

	[TestMethod]
	public void MustNotSquashDifferentCategory()
	{
		var startTime = DateTimeOffset.Now.AddMinutes(-1);

		var actor = new ActorState("rule", "ri", startTime, 1);

		var v1 = new OutputValue(startTime, startTime.AddSeconds(10), false, false, "invalid", "", new KeyValuePair<string, object>[0]);

		actor.OutputValues.Add(v1);

		actor.MissingValue(startTime.AddSeconds(20));

		actor.OutputValues.Count.Should().Be(2);
		actor.OutputValues.Points.Last().EndTime.Should().Be(startTime.AddSeconds(20));

		actor.InvalidValue(startTime.AddSeconds(30));

		actor.OutputValues.Count.Should().Be(3);
		actor.OutputValues.Points.Last().EndTime.Should().Be(startTime.AddSeconds(30));
	}

	[TestMethod]
	public void MustSquashEmptyCategory()
	{
		var now = DateTimeOffset.Now;

		var actor = new ActorState("rule", "ri", now, 1);

		var v1 = new OutputValue(now, now.AddSeconds(10), true, false, "invalid", "", new KeyValuePair<string, object>[0]);

		actor.OutputValues.Add(v1);

		actor.ValidOutput(now.AddMinutes(1), false, Env.Empty.Push());
		actor.OutputValues.Count.Should().Be(1);
		actor.OutputValues.Points.Last().EndTime.Should().Be(now.AddMinutes(1));
	}

	[TestMethod]
	public void OutputVariables_Faulty_Warning_Faulty()
	{
		var now = DateTimeOffset.Now;

		var actor = new ActorState("rule", "ri", now, 1);

		var buffer = new TimeSeries("result", "");

		actor.TimedValues["result"] = buffer;

		actor.OutputValues.VariablesToKeep.Add("val");

		var env = Env.Empty.Push();

		env.Assign("val", 1);

		buffer.AddPoint(new TimedValue(now.AddMinutes(1), true), true);

		actor.ValidOutput(now, true, env);

		actor.ValidOutput(now.AddMinutes(1), true, env);

		actor.OutputValues.Points.Count.Should().Be(1);

		actor.OutputValues.Points[0].Variables[0].Value.Should().Be(1);

		env.Assign("val", 2);

		actor.MissingValue(now.AddMinutes(2));

		actor.OutputValues.Points.Count.Should().Be(2);

		actor.OutputValues.Points[0].Variables[0].Value.Should().Be(1);

		actor.OutputValues.Points[1].Variables.Length.Should().Be(0);

		env.Assign("val", 3);

		//mimic result not failing, buf "percentage faulted" still high, so still faulty
		buffer.AddPoint(new TimedValue(now.AddMinutes(3), false), true);

		actor.ValidOutput(now.AddMinutes(3), true, env);

		actor.OutputValues.Points.Count.Should().Be(3);

		actor.OutputValues.Points[0].Variables[0].Value.Should().Be(1);

		actor.OutputValues.Points[1].Variables.Length.Should().Be(0);

		//still keeps previous value that was faulted
		actor.OutputValues.Points[2].Variables[0].Value.Should().Be(1);
	}

	[TestMethod]
	public void OutputVariables_Warning_Valid_Faulty()
	{
		var now = DateTimeOffset.Now;

		var actor = new ActorState("rule", "ri", now, 1);

		var buffer = new TimeSeries("result", "");

		actor.TimedValues["result"] = buffer;

		actor.OutputValues.VariablesToKeep.Add("val");

		var env = Env.Empty.Push();

		env.Assign("val", 1);

		actor.MissingValue(now.AddMinutes(2));

		actor.MissingValue(now.AddMinutes(5));

		actor.OutputValues.Points.Count.Should().Be(1);

		actor.OutputValues.Points[0].Variables.Length.Should().Be(0);

		env.Assign("val", 2);

		//mimic result not failing, buf "percentage faulted" still high, so still faulty
		buffer.AddPoint(new TimedValue(now.AddMinutes(6), false), true);

		actor.ValidOutput(now.AddMinutes(6), false, env);

		actor.OutputValues.Points.Count.Should().Be(2);

		actor.OutputValues.Points[0].Variables.Length.Should().Be(0);

		actor.OutputValues.Points[1].Variables.Length.Should().Be(1);

		actor.OutputValues.Points[1].Variables[0].Value.Should().Be(2);

		env.Assign("val", 3);

		//mimic result not failing, buf "percentage faulted" still high, so still faulty
		buffer.AddPoint(new TimedValue(now.AddMinutes(7), true), true);

		actor.ValidOutput(now.AddMinutes(7), true, env);

		actor.OutputValues.Points.Count.Should().Be(3);

		actor.OutputValues.Points[0].Variables.Length.Should().Be(0);

		actor.OutputValues.Points[1].Variables.Length.Should().Be(1);

		actor.OutputValues.Points[1].Variables[0].Value.Should().Be(2);

		actor.OutputValues.Points[2].Variables.Length.Should().Be(1);

		actor.OutputValues.Points[2].Variables[0].Value.Should().Be(3);
	}

	[TestMethod]
	public void OutputVariables_Warning_Faulty()
	{
		var now = DateTimeOffset.Now;

		var actor = new ActorState("rule", "ri", now, 1);

		var buffer = new TimeSeries("result", "");

		actor.TimedValues["result"] = buffer;

		actor.OutputValues.VariablesToKeep.Add("val");

		var env = Env.Empty.Push();

		env.Assign("val", 1);

		actor.MissingValue(now.AddMinutes(2));

		actor.MissingValue(now.AddMinutes(5));

		actor.OutputValues.Points.Count.Should().Be(1);

		actor.OutputValues.Points[0].Variables.Length.Should().Be(0);

		env.Assign("val", 2);

		//mimic result not failing, buf "percentage faulted" still high, so still faulty
		buffer.AddPoint(new TimedValue(now.AddMinutes(6), false), true);

		actor.ValidOutput(now.AddMinutes(6), true, env);

		actor.OutputValues.Points.Count.Should().Be(2);

		actor.OutputValues.Points[0].Variables.Length.Should().Be(0);

		actor.OutputValues.Points[1].Variables.Length.Should().Be(1);

		actor.OutputValues.Points[1].Variables[0].Value.Should().Be(2);
	}
}
