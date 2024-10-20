using Pulumi;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using RulesEngine.Pulumi.Components.Helpers;
using RulesEngine.Pulumi.Components.Tags;
using FileShare = Pulumi.AzureNative.Storage.FileShare;

namespace RulesEngine.Pulumi.Components.Storage;

public class Storage : ComponentResource
{

	public Output<string> AccountName { get; set; }

	public Output<string> FileShareName { get; set; }
	public Storage(StorageArgs args) : base($"{StringConstants.ComponentNamespace}:component:Storage", args.Name)
	{
		var storageAccount = new StorageAccount("storageAccount", new StorageAccountArgs
		{
			AccountName =  $"{args.Name.Replace("-", "")}sto",
			EnableHttpsTrafficOnly = true,
			Kind = Kind.StorageV2,
			Sku = new SkuArgs
			{
				Name = SkuName.Standard_GRS
			},
			AccessTier = AccessTier.Hot,
			ResourceGroupName = args.ResourceGroup,
		}, new CustomResourceOptions {Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});

		AccountName = storageAccount.Name;

		var fileShare = new FileShare("fileShare", new FileShareArgs
		{
			AccountName = storageAccount.Name,
			EnabledProtocols = "NFS",
			ResourceGroupName = args.ResourceGroup,
			ShareName = args.CacheFileShareName,
		}, new CustomResourceOptions {Parent = storageAccount, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});

		FileShareName = fileShare.Name;
	}


}