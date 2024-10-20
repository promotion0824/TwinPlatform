using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;
using System.IO;

namespace Authorization.TwinPlatform.Factories;

public static class EntityFactory
{
    public static RoleAssignment ConstructRoleAssignment(UserRoleAssignmentModel model)
    {
        return new RoleAssignment
        {
            Id = model.Id,
            RoleId = model.Role.Id,
            UserId = model.User.Id,
            Expression = (model.Expression ?? string.Empty).Trim(), // convert null expression to empty strings and trim
            Condition = (model.Condition ?? string.Empty).Trim() // convert null conditions to empty strings and trim
        };
    }

    public static RoleAssignment ConstructRoleAssignment(UserRoleAssignmentFileModel fileModel, Guid userId, Guid roleId)
    {
        return new RoleAssignment()
        {
            Id = string.IsNullOrEmpty(fileModel.Id) ? Guid.Empty : Guid.Parse(fileModel.Id),
            RoleId = roleId,
            UserId = userId,
            Expression = fileModel.Expression,
            Condition = fileModel.Condition
        };
    }

    public static Permission ConstructPermission(CreatePermissionModel model, Guid applicationId)
    {
        return new Permission
        {
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            ApplicationId = applicationId
        };
    }

    public static Permission ConstructPermission(PermissionModel model)
    {
        return new Permission
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description ?? string.Empty,
            ApplicationId = model.Application!.Id
        };
    }

    public static Permission ConstructPermission(PermissionFileModel fileModel, Guid applicationId)
    {
        return new Permission
        {
            Id = string.IsNullOrWhiteSpace(fileModel.Id) ? Guid.Empty : Guid.Parse(fileModel.Id),
            Name = fileModel.Name,
            Description = fileModel.Description ?? string.Empty,
            ApplicationId = applicationId,
        };
    }

    public static Role ConstructRole(RoleModel model)
    {
        return new Role
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description ?? string.Empty
        };
    }

    public static Role ConstructRole(CreateRoleModel model)
    {
        return new Role
        {
            Name = model.Name,
            Description = model.Description ?? string.Empty
        };
    }

    public static RolePermission ConstructRolePermission(RolePermissionModel model)
    {
        return new RolePermission
        {
            RoleId = model.RoleId,
            PermissionId = model.Permission.Id
        };
    }

    public static User ConstructUser(UserModel model)
    {
        return new User
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            EmailConfirmed = model.EmailConfirmed,
            CreatedDate = DateTimeOffset.UtcNow,
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            Status = (int)model.Status
        };
    }

    public static User ConstructUser(UserFileModel fileModel)
    {
        return new User
        {
            Id = string.IsNullOrEmpty(fileModel.Id) ? Guid.Empty : Guid.Parse(fileModel.Id),
            FirstName = fileModel.FirstName,
            LastName = fileModel.LastName,
            Email = fileModel.Email,
            EmailConfirmed = false,
            CreatedDate = DateTimeOffset.UtcNow,
            EmailConfirmationToken = Guid.NewGuid().ToString(),
            Status = (int)UserStatus.Active
        };
    }

    public static GroupType ConstructGroupType(GroupTypeModel model)
    {
        return new GroupType()
        {
            Id = model.Id,
            Name = model.Name
        };
    }

    public static Group ConstructGroup(GroupModel model)
    {
        return new Group
        {
            Id = model.Id,
            Name = model.Name,
            GroupTypeId = model.GroupTypeId
        };
    }

    public static Group ConstructGroup(GroupFileModel model, Guid groupTypeId)
    {
        return new Group
        {
            Id = string.IsNullOrEmpty(model.Id) ? Guid.Empty : Guid.Parse(model.Id),
            Name = model.Name,
            GroupTypeId = groupTypeId,
        };
    }

    public static UserGroup ConstructUserGroup(GroupUserModel model)
    {
        return new UserGroup
        {
            Id = model.Id,
            UserId = model.UserId,
            GroupId = model.GroupId
        };
    }

    public static UserGroup ConstructUserGroup(GroupUserFileModel model, Guid userId, Guid groupId)
    {
        return new UserGroup
        {
            Id = string.IsNullOrEmpty(model.Id) ? Guid.Empty : Guid.Parse(model.Id),
            UserId = userId,
            GroupId = groupId
        };
    }

    public static GroupRoleAssignment ConstructGroupRoleAssignment(GroupRoleAssignmentModel model)
    {
        return new GroupRoleAssignment
        {
            Id = model.Id,
            RoleId = model.Role.Id,
            GroupId = model.Group.Id,
            Expression = (model.Expression ?? string.Empty).Trim(), // convert null expression to empty strings and trim
            Condition = (model.Condition ?? string.Empty).Trim() // convert null conditions to empty strings and trim
        };
    }

    public static GroupRoleAssignment ConstructGroupRoleAssignment(GroupRoleAssignmentFileModel fileModel, Guid groupId, Guid roleId)
    {
        return new GroupRoleAssignment()
        {
            Id = string.IsNullOrEmpty(fileModel.Id) ? Guid.Empty : Guid.Parse(fileModel.Id),
            RoleId = roleId,
            GroupId = groupId,
            Expression = fileModel.Expression,
            Condition = fileModel.Condition
        };
    }

    public static ApplicationClient ConstructApplicationClient(ApplicationClientModel model)
    {
        return new ApplicationClient
        {
            Id = model.Id,
            Description = model.Description,
            Name = model.Name,
            ApplicationId = model.Application.Id,
            ClientId = model.ClientId
        };
    }

    public static Application ConstructApplication(ApplicationModel model)
    {
        return new Application
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            SupportClientAuthentication = model.SupportClientAuthentication,
        };
    }

    public static ClientAssignment ConstructClientAssignment(ClientAssignmentModel model)
    {
        return new ClientAssignment()
        {
            Id = model.Id,
            ApplicationClientId = model.ApplicationClient.Id,
            Expression = model.Expression,
            Condition = model.Condition,
        };
    }
}
