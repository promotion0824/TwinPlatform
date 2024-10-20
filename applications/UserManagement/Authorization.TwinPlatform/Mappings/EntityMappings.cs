using Authorization.Common.Models;
using Authorization.TwinPlatform.Persistence.Entities;
using AutoMapper;

namespace Authorization.TwinPlatform.Mappings;

/// <summary>
/// Class that maps the Entity Model with the DTO Model
/// </summary>
public class EntityMappings : Profile
{
    public EntityMappings()
    {
        CreateMap<Role, RoleModel>();
        CreateMap<Role, RoleModelWithPermissions>()
            .ForMember(m => m.Permissions, opt => opt.MapFrom(x => x.RolePermission.Select(r => r.Permission)));

        CreateMap<User, UserModel>();
        CreateMap<User, UserFileModel>();
        CreateMap<Permission, PermissionModel>()
            .ForMember(m => m.Application, opt => opt.MapFrom(x => x.Application));

        CreateMap<Permission, PermissionFileModel>()
            .ForMember(m => m.Application, opt => opt.MapFrom(x => x.Application.Name));
        CreateMap<RoleAssignment, UserRoleAssignmentModel>();
        CreateMap<RolePermission, RolePermissionModel>();
        CreateMap<GroupType, GroupTypeModel>();
        CreateMap<Group, GroupModel>()
            .ForMember(g => g.GroupType, opt => opt.MapFrom(x => x.GroupType))
            .ForMember(m => m.Users, opt => opt.MapFrom(x => x.UserGroups.Select(s => s.User)));
        CreateMap<Group, GroupFileModel>()
            .ForMember(g => g.GroupType, opt => opt.MapFrom(x => x.GroupType.Name));

        CreateMap<UserGroup, GroupUserModel>();
        CreateMap<UserGroup, GroupUserFileModel>()
            .ForMember(g => g.UserEmail, opt => opt.MapFrom(x => x.User.Email))
            .ForMember(g => g.Group, opt => opt.MapFrom(x => x.Group.Name));

        CreateMap<GroupRoleAssignment, GroupRoleAssignmentModel>();

        CreateMap<RoleAssignment, UserRoleAssignmentFileModel>()
            .ForMember(m => m.RoleName, opt => opt.MapFrom(x => x.Role.Name))
            .ForMember(m => m.UserEmail, opt => opt.MapFrom(x => x.User.Email));

        CreateMap<GroupRoleAssignment, GroupRoleAssignmentFileModel>()
            .ForMember(m => m.RoleName, opt => opt.MapFrom(x => x.Role.Name))
            .ForMember(m => m.GroupName, opt => opt.MapFrom(x => x.Group.Name));

        CreateMap<Application, ApplicationModel>();
        CreateMap<ApplicationClient, ApplicationClientModel>()
            .ForMember(m => m.Application, opt => opt.MapFrom(x => x.Application));

        CreateMap<ClientAssignmentPermission, ClientAssignmentPermissionModel>();
        CreateMap<ClientAssignment, ClientAssignmentModel>()
            .ForMember(a=>a.ApplicationClient, opt=>opt.MapFrom(x=>x.ApplicationClient))
            .ForMember(a=>a.Permissions, opt=>opt.MapFrom(x=>x.ClientAssignmentPermissions.Select(s=>s.Permission)));
    }
}
