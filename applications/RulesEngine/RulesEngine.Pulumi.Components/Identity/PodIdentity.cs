using Pulumi;
using Pulumi.Crds.Aadpodidentity.V1;
using Pulumi.Kubernetes.Types.Inputs.Aadpodidentity.V1;
using Pulumi.Kubernetes.Types.Inputs.Meta.V1;
using RulesEngine.Pulumi.Components.Helpers;

namespace RulesEngine.Pulumi.Components.Identity;

public class PodIdentity : ComponentResource
{
	public PodIdentity(PodIdentityArgs args) : base($"{StringConstants.ComponentNamespace}:component:PodIdentity",
		args.Name)
	{

		var azureIdentity = new AzureIdentity($"{args.Name}-azure-id", new AzureIdentityArgs
		{
			Metadata = new ObjectMetaArgs
			{
				Labels = args.AdditionalLabels,
				Namespace = args.Namespace,
			},
			Spec = new AzureIdentitySpecArgs
			{
				ClientId = args.IdentityClientId,
				ResourceId = args.IdentityId
			}
		});

		Identity = azureIdentity.Metadata.Apply(i => i.Name);


		var azureIdentityBinding = new AzureIdentityBinding($"{args.Name}-azure-id-binding", new AzureIdentityBindingArgs
		{
			Metadata = new ObjectMetaArgs
			{
				Labels = args.AdditionalLabels,
				Namespace = args.Namespace,
			},
			Spec = new AzureIdentityBindingSpecArgs
			{
				AzureIdentity = $"{args.Name}-azure-id",
				Selector = args.IdSelector
			}
		});

		IdentityBinding = azureIdentityBinding.Metadata.Apply(i => i.Name);

	}

	[Output] public Output<string> Identity { get; set; }
	[Output] public Output<string> IdentityBinding{ get; set; }
}