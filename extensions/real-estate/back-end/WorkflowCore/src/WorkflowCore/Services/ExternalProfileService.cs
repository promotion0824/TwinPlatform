using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Entities;
using WorkflowCore.Services.MappedIntegration.Models;

namespace WorkflowCore.Services;

public interface IExternalProfileService
{
    Task<List<MappedUserProfile>> GetAssigneeExternalProfiles();
}

public class ExternalProfileService : IExternalProfileService
{
    private readonly WorkflowContext _workflowContext;
    // we should filter out the Mapped Connector user profile
    // this user profile is used by Mapped
    public const string MappedConnectorUserProfile = "Mapped Connector";

    public ExternalProfileService(WorkflowContext workflowContext)
    {
        _workflowContext = workflowContext;
    }
    public async Task<List<MappedUserProfile>> GetAssigneeExternalProfiles()
    {
       
        var externalProfileEntities = await _workflowContext.ExternalProfiles
                                        .Where(x => !x.Name.Contains(MappedConnectorUserProfile)).ToListAsync();
        var externalProfiles = MappedUserProfile.MapFrom(externalProfileEntities);
        return externalProfiles;
    }
}

