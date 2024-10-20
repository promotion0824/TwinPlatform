using Pulumi;

namespace RulesEngine.Pulumi.Components.Identity;

public class PodIdentityArgs
{
	public PodIdentityArgs(string name, Input<string> ns, Input<string> identityId, Input<string> identityClientId,
		Input<string> idSelector, InputMap<string> additionalLabels)
	{
		Namespace = ns;
		IdentityId = identityId;
		IdentityClientId = identityClientId;
		IdSelector = idSelector;
		AdditionalLabels = additionalLabels;
		Name = name;
	}

	public string Name { get; }
	public Input<string> Namespace { get; }
	public Input<string> IdentityId { get; }
	public Input<string> IdentityClientId { get; }
	public Input<string> IdSelector { get; }
	public InputMap<string> AdditionalLabels { get; }
}