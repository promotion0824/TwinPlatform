using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Willow.Rules.Model;
using WillowRules.Extensions;

namespace Willow.Rules.Test;

[TestClass]
public class ExtensionTests
{
	[TestMethod]
	public void TrimModelIdMustWork()
	{
		var modelId = "dtmi:com:willowinc:TerminalUnit;1".TrimModelId();

		modelId.Should().Be("TerminalUnit");
	}

	[TestMethod]
	public void MustUpdateVariableNameInExpression()
	{
		var parameter = new RuleParameter()
		{
			PointExpression = "myVariable + othervariable"
		};

		bool matched = parameter.MatchVariableName("myVariable");

		matched.Should().BeTrue();
	}
}
