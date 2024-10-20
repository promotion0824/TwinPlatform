using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Willow.Rules.Model;

namespace WillowRules.Test.Models;

[TestClass]
public class NamedPointTests
{
	[TestMethod]
	public void NamedPointFullnameTests()
	{
		string json = @"[
		{
			""Id"": ""PNTNPm28d1Vtr95hFfp9vdkSu"",
			""VariableName"": ""heating air damper position cmd"",
			""Unit"": ""%"",
			""FullName"": ""heating air damper position cmd"",
			""ModelId"": ""dtmi:com:willowinc:HotDeckInletAirDamperPositionActuator;1"",
			""Locations"": [
				{
					""Id"": ""PNTNPm28d1Vtr95hFfp9vdkSu"",
					""Name"": ""heating air damper position cmd"",
					""ModelId"": ""dtmi:com:willowinc:HotDeckInletAirDamperPositionActuator;1""
				},
				{
					""Id"": ""NAU-17-DD-A-5"",
					""Name"": ""DD-A-5"",
					""ModelId"": ""dtmi:com:willowinc:DualDuctVAVBox;1""
				},
				{
					""Id"": ""NAU-17-L02-200A"",
					""Name"": ""Common Space 200 A"",
					""ModelId"": ""dtmi:com:willowinc:Room;1""
				},
				{
					""Id"": ""NAU-17-L02"",
					""Name"": ""Level 02"",
					""ModelId"": ""dtmi:com:willowinc:Level;1""
				}
			]
		},
		{
			""Id"": ""PNT6gtohLTShs1YjdDGWxmUAA"",
			""VariableName"": ""heating air damper position cmd"",
			""Unit"": ""%"",
			""FullName"": ""heating air damper position cmd"",
			""ModelId"": ""dtmi:com:willowinc:HotDeckInletAirDamperPositionActuator;1"",
			""Locations"": [
				{
					""Id"": ""PNT6gtohLTShs1YjdDGWxmUAA"",
					""Name"": ""heating air damper position cmd"",
					""ModelId"": ""dtmi:com:willowinc:HotDeckInletAirDamperPositionActuator;1""
				},
				{
					""Id"": ""NAU-17-DD-A-5A"",
					""Name"": ""DD-A-5"",
					""ModelId"": ""dtmi:com:willowinc:DualDuctVAVBox;1""
				},
				{
					""Id"": ""NAU-17-L02-200B"",
					""Name"": ""Common Space 200 B"",
					""ModelId"": ""dtmi:com:willowinc:Room;1""
				},
				{
					""Id"": ""NAU-17-L02"",
					""Name"": ""Level 02"",
					""ModelId"": ""dtmi:com:willowinc:Level;1""
				}
			]
		}]";

		var pointData = JsonConvert.DeserializeObject<List<NamedPoint>>(json);

		Assert.IsNotNull(pointData);
		Assert.IsTrue(pointData.Count > 0);

		var singleList = new List<NamedPoint>() { pointData[0] };

		Assert.IsTrue(singleList.ResolveAmbiguities().FirstOrDefault()!.FullName == "[heating air damper position cmd]");
		Assert.IsTrue(singleList.ResolveAmbiguities(includeBracketFormat: false).FirstOrDefault()!.FullName == "heating air damper position cmd");

		var updatedPointData = pointData.ResolveAmbiguities().ToList();

		Assert.IsTrue(updatedPointData[0].FullName == "[Common Space 200 A].[DD-A-5].[heating air damper position cmd]");
		Assert.IsTrue(updatedPointData[1].FullName == "[Common Space 200 B].[DD-A-5].[heating air damper position cmd]");

		updatedPointData = pointData.ResolveAmbiguities(includeBracketFormat: false).ToList();

		Assert.IsTrue(updatedPointData[0].FullName == "Common Space 200 A.DD-A-5.heating air damper position cmd");
		Assert.IsTrue(updatedPointData[1].FullName == "Common Space 200 B.DD-A-5.heating air damper position cmd");
	}
}
