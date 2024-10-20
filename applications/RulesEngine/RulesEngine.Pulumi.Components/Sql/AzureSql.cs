using Pulumi;
using Pulumi.AzureNative.Sql;
using Pulumi.AzureNative.Sql.Inputs;
using RulesEngine.Pulumi.Components.Helpers;
using RulesEngine.Pulumi.Components.Tags;

namespace RulesEngine.Pulumi.Components.Sql;

public class AzureSql : ComponentResource
{
	public Output<string> RulesEngineConnectionString { get; set; }
	public Output<string?> DiagnosticsLocation { get; set; }


	public AzureSql(AzureSqlArgs args)
		: base($"{StringConstants.ComponentNamespace}:component:AzureSql", args.Name)
	{

		var sqlServer = new Server($"{args.Name}-sql", new ServerArgs
		{
			ResourceGroupName = args.ResourceGroup,
			Version = "12.0",
			Administrators = new ServerExternalAdministratorArgs
			{
				AzureADOnlyAuthentication = true,
				Login = args.AdminGroupName,
				AdministratorType = AdministratorType.ActiveDirectory,
				PrincipalType = PrincipalType.Group,
				Sid = args.AdminGroupSid,
				TenantId = args.AdminGroupTenantId
			},

			PublicNetworkAccess = "Enabled", // will move to disabled for prod


			Tags = args.Tags
		}, new CustomResourceOptions {Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});

		var sqlDatabase = new Database(args.RulesEngineDbName, new DatabaseArgs
		{
			DatabaseName = args.RulesEngineDbName,
			ServerName = sqlServer.Name,
			ResourceGroupName = args.ResourceGroup,
			Sku = new SkuArgs
			{
				Name = "S0",
				Tier = "Standard",
				Capacity = 10
			},

			AutoPauseDelay = 60 * 60,
			MaxSizeBytes = 10L * 1024 * 1024 * 1024,  // 10GB

			//Kind = "v12.0,user,vcore",

			Tags = args.Tags
		}, new CustomResourceOptions {Parent = this, IgnoreChanges = TagConstants.TagChangesToIgnore.ToList(), Protect = true});


		var diagnosticSetting =  new DiagnosticsSettingBuilder($"{args.Name}-sql-diag", sqlDatabase.Id, args.WorkSpaceId, args.LogRetention)
			.WithParent(sqlDatabase)
			.WithCategory("allLogs")
			.Build();
		DiagnosticsLocation = diagnosticSetting.WorkspaceId;

		RulesEngineConnectionString = Output.Tuple(sqlServer.FullyQualifiedDomainName, sqlDatabase.Name)
			.Apply(i => $"Server=tcp:{i.Item1};Database={i.Item2};Authentication=Active Directory Managed Identity;");



		// TODO: Get user creation working: SqlUsers.CreateDatabaseContainedUser(connectionString, containerIdentity);

	}
}