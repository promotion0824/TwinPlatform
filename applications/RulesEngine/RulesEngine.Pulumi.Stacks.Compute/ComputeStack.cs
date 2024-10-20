using Pulumi;
using Pulumi.Crds.Aadpodidentity.V1;
using Pulumi.Kubernetes.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Aadpodidentity.V1;
using Pulumi.Kubernetes.Types.Inputs.Core.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using RulesEngine.Pulumi.Components.Identity;
using RulesEngine.Pulumi.Components.Labels;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Stacks.Compute;

public class ComputeStack : Stack
{
	public ComputeStack()
	{

		var config = new Config();
		var customer = config.Require("customer");
		var environment = config.Require("environment");

		var additionalLabels = new LabelBuilder("RulesEngineCompute", customer, environment).Build();

		var name = Deployment.Instance.StackName.Replace(".", "-");

		var webRepository = config.Require("webRepository");
		var webTag = config.Require("webTag");

		var processorRepository = config.Require("processorRepository");
		var processorTag = config.Require("processorTag");

		var frontendRepository = config.Require("frontendRepository");
		var frontendTag = config.Require("frontendTag");

		var resourceStackLocation = config.Require("resourceStack");
		var resourceStack = new StackReference(resourceStackLocation);
		var identityId = resourceStack.RequireOutput("IdentityId").Apply(v => v.ToString());
		var identityClientId = resourceStack.RequireOutput("IdentityClientId").Apply(v => v.ToString());

		var appNamespace = new Namespace($"rules-{name}", new NamespaceArgs
		{
			Metadata = new ObjectMetaArgs
			{
				Labels = additionalLabels,
				Name = $"rules-{name}",
			},
		}, new CustomResourceOptions {Parent = this});

		var podIdentity = new PodIdentity(new PodIdentityArgs(name, appNamespace.Metadata.Apply(x => x.Name),
			identityId!, identityClientId!, "rules-id", additionalLabels));

		Identity = podIdentity.Identity;
		IdentityBinding = podIdentity.IdentityBinding;


	}

	[Output] public Output<string> Identity { get; set; }
	[Output] public Output<string> IdentityBinding{ get; set; }
}