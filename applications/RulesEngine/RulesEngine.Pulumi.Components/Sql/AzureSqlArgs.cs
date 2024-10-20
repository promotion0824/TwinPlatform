using Pulumi;

namespace RulesEngine.Pulumi.Components.Sql;

public class AzureSqlArgs : BaseArgs
{
	public Input<string> WorkSpaceId { get; }
	public Input<string> AdminGroupName { get; set; }
	public Input<string> AdminGroupSid { get; set; }
	public Input<string> AdminGroupTenantId { get; set; } = "d43166d1-c2a1-4f26-a213-f620dba13ab8";
	public string RulesEngineDbName { get; set; } = "RulesEngineDb";


	public AzureSqlArgs(BaseArgs baseArgs, Input<string> workSpaceId, Input<string> adminGroupName, Input<string> adminGroupSid) : base(baseArgs)
	{
		WorkSpaceId = workSpaceId;
		AdminGroupName = adminGroupName;
		AdminGroupSid = adminGroupSid;
	}
}