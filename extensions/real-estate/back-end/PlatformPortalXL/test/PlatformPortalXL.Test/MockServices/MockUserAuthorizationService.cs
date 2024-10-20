using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using PlatformPortalXL.Auth.Permissions;
using Willow.Batch;
namespace PlatformPortalXL.Test.MockServices;

public class MockUserAuthorizationService: IUserAuthorizationService
{
    public Task<AuthorizationResponse> GetAuthorizationResponse(string userName)
    {
        if (userName.Contains("admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(new AuthorizationResponse()
            {
                Permissions = new List<AuthorizedPermission>()
                    { new AuthorizedPermission() { Name = nameof(CanActAsCustomerAdmin) } }
            });
        }

        return Task.FromResult(new AuthorizationResponse()
        {
            Permissions = new List<AuthorizedPermission>()
                { new AuthorizedPermission() { Name = "normalUser" } }
        });
    }

    public Task<ListResponse<GroupModel>> GetApplicationGroupsAsync(FilterPropertyModel filterModel)
    {
        return Task.FromResult(new ListResponse<GroupModel>(new List<GroupModel>()
        {
            new GroupModel() { Id = Guid.Parse("902a462f-45c8-4976-9e91-10727e5c3ff4"), Name = "TestWorkGroup1", Users = new List<UserModel>(new List<UserModel>() { new() { Id = Guid.NewGuid() } }) },
            new GroupModel() { Id = Guid.Parse("802a462f-45c8-4976-9e91-10727e5c3ff4"), Name = "TestWorkGroup2", Users = new List<UserModel>(new List<UserModel>() { new() { Id = Guid.Empty} }) }
        }));
    }

    public Task<ListResponse<UserModel>> GetUsersAsync(FilterPropertyModel filterModel)
    {
        return Task.FromResult(new ListResponse<UserModel>(new List<UserModel>()
        {
            new UserModel() { Email = "admin@test.com"}
        }));
    }

    public Task<UserModel> GetUserByEmailAsync(string email)
    {
        throw new System.NotImplementedException();
    }

    public Task<ListResponse<UserModel>> GetListOfUserByIds(string[] userIds)
    {
        throw new System.NotImplementedException();
    }


    public async Task<BatchDto<GroupModel>> GetApplicationGroupsAsync(BatchRequestDto batchRequest)
    {
        throw new System.NotImplementedException();
    }

    public async Task<BatchDto<GroupModel>> GetApplicationGroupsByUserAsync(string userId, BatchRequestDto batchRequest)
    {
        var response = new BatchDto<GroupModel>()
        {
            Total = 2,
            Items =
            [
                new GroupModel() { Id = Guid.Parse("902a462f-45c8-4976-9e91-10727e5c3ff4"),Name = "TestWorkGroup1", Users = new List<UserModel>(new List<UserModel>() { new() { Id = Guid.NewGuid() } }) },
                new GroupModel() { Id = Guid.Parse("802a462f-45c8-4976-9e91-10727e5c3ff4"), Name = "TestWorkGroup2", Users = new List<UserModel>(new List<UserModel>() { new() { Id = Guid.Empty } }) }
            ]
        };

        return response;
    }
}
