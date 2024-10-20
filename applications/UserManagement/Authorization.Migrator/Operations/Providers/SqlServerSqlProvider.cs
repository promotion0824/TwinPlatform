using System;
using System.Collections.Generic;

namespace Authorization.Migrator.Migrations.Operations.Providers;

/// <summary>
/// Class that contains SqlServer implementation of SQL for Non-EF objects used by Authorization Engine
/// </summary>
public static class SqlServerSqlProvider
{
	private static readonly Dictionary<string, string> _dbObjects = new();
	static SqlServerSqlProvider()
	{
		_dbObjects.Add("UsersWithinGroupFunction_Up",
			@"CREATE function [dbo].[GetUsersWithinGroup](@groupName nvarchar(100))
									returns table
									as
									return(
									select u.* from Users U 
										inner join UserGroups UG on U.Id = UG.UserId
										inner join Groups G on UG.GroupId = g.Id
										where LOWER(g.Name) = LOWER(@groupName)
									)");
		_dbObjects.Add("UsersWithinGroupFunction_Down", @"drop function [dbo].[GetUsersWithinGroup]");

		_dbObjects.Add("GetRoleAssignmentsByUser_Up", @"CREATE function [dbo].[GetRoleAssignmentsByUser](@userEmail nvarchar(100),@ResourceId nvarchar(100))
						returns table
						as 
						return
						(
						Select RA.UserId,RA.RoleId,RA.ResourceId,RA.Condition from Roles R
						inner join RoleAssignments RA on RA.RoleId = R.Id
						inner join Users U on U.Id = RA.UserId
						where u.Email = @userEmail and (@ResourceId is null or RA.ResourceId = @ResourceId)
						union
						Select U.Id,GRA.RoleId,GRA.ResourceId,GRA.Condition from Roles R
						inner join GroupRoleAssignments GRA on GRA.RoleId = R.Id
						inner join UserGroups UG on UG.GroupId = GRA.GroupId
						inner join Users U on U.Id = UG.UserId
						where U.Email = @userEmail and (@ResourceId is null or GRA.ResourceId = @ResourceId)
						)");

		_dbObjects.Add("GetRoleAssignmentsByUser_Down", @"drop function [dbo].[GetRoleAssignmentsByUser]");

        _dbObjects.Add("GetRoleAssignmentsByUser_v1_Up", @"CREATE function [dbo].[GetRoleAssignmentsByUser](@userEmail nvarchar(100))
						returns table
						as 
						return
						(
						Select RA.UserId,RA.RoleId,RA.Expression,RA.Condition from Roles R
						inner join RoleAssignments RA on RA.RoleId = R.Id
						inner join Users U on U.Id = RA.UserId
						where u.Email = @userEmail
						union
						Select U.Id,GRA.RoleId,GRA.Expression,GRA.Condition from Roles R
						inner join GroupRoleAssignments GRA on GRA.RoleId = R.Id
						inner join UserGroups UG on UG.GroupId = GRA.GroupId
						inner join Users U on U.Id = UG.UserId
						where U.Email = @userEmail
						)");
        _dbObjects.Add("GetRoleAssignmentsByUser_v1_Down", @"drop function [dbo].[GetRoleAssignmentsByUser]");
    }

    /// <summary>
    /// Method to get the Sql query string for requested database object by name
    /// </summary>
    /// <param name="name">Name of the Database Object</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static string GetSql(string name)
	{
		if (_dbObjects.TryGetValue(name, out var sql))
		{
			return sql;
		}
		else
		{
			throw new NotImplementedException();
		}
	}
}

