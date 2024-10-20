using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Persistence.Contexts;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace Authorization.TwinPlatform.Services;
public class GroupTypeService : IGroupTypeService
{

    private readonly TwinPlatformAuthContext _authContext;
    private readonly IMapper _mapper;

    /// <summary>
    /// Instantiate a new instance for <see cref="GroupTypeService"/>
    /// </summary>
    /// <param name="authContext"></param>
    /// <param name="mapper"></param>
    public GroupTypeService(TwinPlatformAuthContext authContext, IMapper mapper)
    {
        _authContext = authContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Get the group type record by name.
    /// </summary>
    /// <param name="name">Name of the group type.</param>
    /// <returns>Instance of <see cref="GroupTypeModel"/></returns>
    public async Task<GroupTypeModel> GetGroupTypeByName(string name)
    {
        var response = await _authContext.GroupTypes
                        .Where(g => g.Name == name)
                        .ProjectTo<GroupTypeModel>(_mapper.ConfigurationProvider).FirstAsync();
        return response;
    }

    /// <summary>
    /// Get list of group types.
    /// </summary>
    /// <returns>List of <see cref="GroupTypeModel"/></returns>
    public async Task<List<GroupTypeModel>> GetListAsync()
    {
        var response = await _authContext.GroupTypes
                        .ProjectTo<GroupTypeModel>(_mapper.ConfigurationProvider).ToListAsync();
        return response;
    }
}
