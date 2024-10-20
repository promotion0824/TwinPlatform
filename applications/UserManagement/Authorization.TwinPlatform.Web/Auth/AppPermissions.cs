namespace Authorization.TwinPlatform.Web.Auth;

/// <summary>
/// Record to hold Application Permissions
/// </summary>
public record AppPermissions
{
    public const string ExtensionName = "AuthorizationWeb";
	#region User
	public const string CanReadUser = nameof(CanReadUser);
	public const string CanCreateUser = nameof(CanCreateUser);
	public const string CanEditUser = nameof(CanEditUser);
	public const string CanDeleteUser = nameof(CanDeleteUser);
	#endregion

	#region Role 
	public const string CanReadRole = nameof(CanReadRole);
	public const string CanCreateRole = nameof(CanCreateRole);
	public const string CanEditRole = nameof(CanEditRole);
	public const string CanDeleteRole = nameof(CanDeleteRole);
	public const string CanAssignPermission = nameof(CanAssignPermission);
	public const string CanRemovePermission = nameof(CanRemovePermission);
	#endregion

	#region Group
	public const string CanReadGroup = nameof(CanReadGroup);
	public const string CanViewAdGroup = nameof(CanViewAdGroup); // special permission => assigned only to internal users
	public const string CanManageAdGroup = nameof(CanManageAdGroup); // special permission => visible only to Super Admins
	public const string CanCreateGroup = nameof(CanCreateGroup);
	public const string CanEditGroup = nameof(CanEditGroup);
	public const string CanDeleteGroup = nameof(CanDeleteGroup);
	public const string CanAssignUser = nameof(CanAssignUser);
	public const string CanRemoveUser = nameof(CanRemoveUser);
	#endregion

	#region Permission
	public const string CanReadPermission = nameof(CanReadPermission);
	public const string CanCreatePermission = nameof(CanCreatePermission);
	public const string CanEditPermission = nameof(CanEditPermission);
	public const string CanDeletePermission = nameof(CanDeletePermission);
	#endregion

	#region Assignment
	public const string CanReadAssignment = nameof(CanReadAssignment);
	public const string CanCreateAssignment = nameof(CanCreateAssignment);
	public const string CanEditAssignment = nameof(CanEditAssignment);
	public const string CanDeleteAssignment = nameof(CanDeleteAssignment);
    #endregion

    #region ImportExport
    public const string CanImportData = nameof(CanImportData);
    public const string CanExportData = nameof(CanExportData);
    #endregion

    #region Application
    public const string CanReadApplication = nameof(CanReadApplication);
    #endregion

    #region Application Clients
    public const string CanReadApplicationClient = nameof(CanReadApplicationClient);
    public const string CanCreateApplicationClient = nameof(CanCreateApplicationClient);
    public const string CanEditApplicationClient = nameof(CanEditApplicationClient);
    public const string CanDeleteApplicationClient = nameof(CanDeleteApplicationClient);
    #endregion

    #region Application Client Permission Assignment
    public const string CanReadClientAssignment = nameof(CanReadClientAssignment);
    public const string CanCreateClientAssignment = nameof(CanCreateClientAssignment);
    public const string CanEditClientAssignment = nameof(CanEditClientAssignment);
    public const string CanDeleteClientAssignment = nameof(CanDeleteClientAssignment);
    #endregion
}
