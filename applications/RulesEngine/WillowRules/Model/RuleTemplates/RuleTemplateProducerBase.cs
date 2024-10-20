using System;
using System.Collections.Generic;

namespace Willow.Rules.Model.RuleTemplates;

/// <summary>
/// Rule template producers have a name and a factory
/// </summary>
public class RuleTemplateProducerBase
{
	public string Id { get; protected init; }

	public string RuletemplateName { get; protected init; }

	public string RuleTemplateDescription { get; }

	/// <summary>
	/// Create a new instance of the template
	/// </summary>
	public Func<ICollection<RuleUIElement>, RuleTemplate> Factory { get; protected init; }

	protected RuleTemplateProducerBase(string id, string ruleTemplateName,
		string ruleTemplateDescription,
		Func<ICollection<RuleUIElement>, RuleTemplate> factory)
	{
		this.Id = id ?? throw new ArgumentNullException(nameof(id));
		this.RuletemplateName = ruleTemplateName ?? throw new ArgumentNullException(nameof(ruleTemplateName));
		this.RuleTemplateDescription = ruleTemplateDescription ?? throw new ArgumentNullException(nameof(ruleTemplateDescription));
		this.Factory = factory;
	}
}
