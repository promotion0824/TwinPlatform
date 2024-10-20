namespace Authorization.TwinPlatform.Queries;

/// <summary>
/// Query Class to count User roles based on permission, extension and Resource
/// </summary>
public class GetAssignedRoleCountQuery : QueryBase<int>
{
	private const string Query = @"
	select Count(1) as NumberOfRolesWithPermission  from GetRolesByUserAssignments(@Email, @Resource) R
		inner join RolePermissions RP on RP.RoleId = R.Id
		inner join Permissions P on P.Id = Rp.PermissionId
	WHERE
		P.Extension = @Extension AND
		P.Name = @Permission";

	public GetAssignedRoleCountQuery(
		string userEmail,
		string extension,
		string permission,
		string? resource) : base(Query)
	{
		Parameters = new
		{
			Email = userEmail,
			Extension = extension,
			Resource = resource,
			Permission = permission
		};
	}
}