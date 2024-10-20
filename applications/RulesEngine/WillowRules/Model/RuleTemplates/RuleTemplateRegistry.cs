using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// A registry of all the rule templates from any loaded assemblies containing them
/// </summary>
public class RuleTemplateRegistry
{
	private readonly ILogger<RuleTemplateRegistry> logger;

	public RuleTemplateRegistry(ILogger<RuleTemplateRegistry> logger)
	{
		this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Gets all the rule template producers
	/// </summary>
	/// <remarks>
	/// Do we need this? Maps names to constructors ...
	/// </remarks>
	public IEnumerable<RuleTemplateProducerBase> GetAll()
	{
		yield return new RuleTemplateProducer<RuleTemplateAnyFault>(RuleTemplateAnyFault.ID,
			RuleTemplateAnyFault.NAME, RuleTemplateAnyFault.DESCRIPTION,
			(x) => new RuleTemplateAnyFault(x.ToArray()));

		yield return new RuleTemplateProducer<RuleTemplateAnyHysteresis>
			(RuleTemplateAnyHysteresis.ID,
			RuleTemplateAnyHysteresis.NAME, RuleTemplateAnyHysteresis.DESCRIPTION,
			(x) => new RuleTemplateAnyHysteresis("F", x.ToArray())
			);

		yield return new RuleTemplateProducer<RuleTemplateFrequency>(RuleTemplateFrequency.ID,
			RuleTemplateFrequency.NAME, RuleTemplateFrequency.DESCRIPTION,
			(x) => new RuleTemplateFrequency(x.ToArray()));

		yield return new RuleTemplateProducer<RuleTemplateUnchanging>(RuleTemplateUnchanging.ID,
			RuleTemplateUnchanging.NAME, RuleTemplateUnchanging.DESCRIPTION,
			(x) => new RuleTemplateUnchanging(x.ToArray()));

		yield return new RuleTemplateProducer<RuleTemplateCalculatedPoint>(RuleTemplateCalculatedPoint.ID,
			RuleTemplateCalculatedPoint.NAME, RuleTemplateCalculatedPoint.DESCRIPTION,
			(x) => new RuleTemplateCalculatedPoint(x.ToArray()));
	}
}
