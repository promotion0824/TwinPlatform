using System;
using System.Collections.Generic;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Produces RuleTemplate instances for a single rule template
/// </summary>
/// <remarks>
/// Generic just for convenience, maybe ...
/// </remarks>
public class RuleTemplateProducer<T> : RuleTemplateProducerBase where T : RuleTemplate
{
	public RuleTemplateProducer(string id, string ruletemplateName,
		string ruleTemplateDescription,
		Func<ICollection<RuleUIElement>, T> factory)
		: base(id, ruletemplateName, ruleTemplateDescription, factory)
	{
	}
}
