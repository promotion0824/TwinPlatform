using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Controllers.Request;
using WorkflowCore.Models;
using WorkflowCore.Repository;
using Willow.ExceptionHandling.Exceptions;
using WorkflowCore.Services.Apis;
using System.Linq;
namespace WorkflowCore.Services
{
    public interface IWorkgroupService
    {
        Task<List<Workgroup>> GetWorkgroups(Guid siteId, bool includeMemberIds);
        Task<List<Workgroup>> GetWorkgroups(string siteName, bool includeMemberIds);
        Task<Workgroup> GetWorkgroup(Guid siteId, Guid workgroupId, bool includeMemberIds);
        Task<Workgroup> CreateWorkgroup(Guid siteId, CreateWorkgroupRequest createWorkgroupRequest);
        Task UpdateWorkgroup(Guid siteId, Guid workgroupId, UpdateWorkgroupRequest updateWorkgroupRequest);
        Task<bool> DeleteWorkgroup(Guid siteId, Guid workgroupId);
    }

    public class WorkgroupService : IWorkgroupService
    {
        private readonly IWorkflowRepository _repository;
        private readonly IDigitalTwinServiceApi _digitalTwinServiceApi;

        public WorkgroupService(IWorkflowRepository repository, IDigitalTwinServiceApi digitalTwinServiceApi)
        {
            _repository = repository;
            _digitalTwinServiceApi = digitalTwinServiceApi;
        }

        public async Task<Workgroup> CreateWorkgroup(Guid siteId, CreateWorkgroupRequest createWorkgroupRequest)
        {
            var workgroup = new Workgroup
            {
                Id = Guid.NewGuid(),
                Name = createWorkgroupRequest.Name,
                SiteId = siteId,
                MemberIds = createWorkgroupRequest.MemberIds
            };
            await _repository.CreateWorkgroup(workgroup);
            await _repository.UpdateWorkgroupMembers(workgroup.Id, createWorkgroupRequest.MemberIds);
            return workgroup;
        }

        public async Task<List<Workgroup>> GetWorkgroups(Guid siteId, bool includeMemberIds)
        {
            var workgroups =  await _repository.GetWorkgroups(siteId, includeMemberIds);

            var siteTwinId = (await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(siteId, new List<Guid>() { siteId })).FirstOrDefault();
            var siteName = siteTwinId?.Name ?? string.Empty;

            foreach (var workgroup in workgroups)
            {
                workgroup.Name = $"{siteName} - {workgroup.Name}";
            }

            return workgroups;
        }

        public async Task<List<Workgroup>> GetWorkgroups(string siteName, bool includeMemberIds)
        {
            var workgroups =  await _repository.GetWorkgroups(siteName, includeMemberIds);

            var siteId = workgroups.FirstOrDefault(x => x.SiteId != Guid.Empty)?.SiteId;

            if (siteId.HasValue)
            {
                var siteTwinIds = await _digitalTwinServiceApi.GetTwinIdsByUniqueIdsAsync(siteId.Value,
                    workgroups.Where(x => x.SiteId != Guid.Empty).Select(x => x.SiteId).Distinct());

                var siteWorkgroups = workgroups.GroupBy(x => x.SiteId);

                foreach(var siteWorkgroup in siteWorkgroups)
                {
                    var siteTwinId = siteTwinIds.FirstOrDefault(x => x.UniqueId == siteWorkgroup.Key.ToString());
                    var prefix = siteTwinId?.Name ?? string.Empty;

                    foreach (var workgroup in siteWorkgroup)
                    {
                        workgroup.Name = $"{prefix} - {workgroup.Name}";
                    }
                }
            }
           
            return workgroups;
        }

        public async Task<Workgroup> GetWorkgroup(Guid siteId, Guid workgroupId, bool includeMemberIds)
        {
            return await _repository.GetWorkgroup(siteId, workgroupId, includeMemberIds);
        }

        public async Task<bool> DeleteWorkgroup(Guid siteId, Guid workgroupId)
        {
            return await _repository.DeleteWorkgroup(siteId, workgroupId);
        }

        public async Task UpdateWorkgroup(Guid siteId, Guid workgroupId, UpdateWorkgroupRequest updateWorkgroupRequest)
        {
            var workgroup = await _repository.GetWorkgroup(siteId, workgroupId, false);
            if (workgroup == null)
            {
                throw new NotFoundException(new { WorkgroupId = workgroupId });
            }
            workgroup.Name = updateWorkgroupRequest.Name;
            await _repository.UpdateWorkgroup(workgroup);
            await _repository.UpdateWorkgroupMembers(workgroup.Id, updateWorkgroupRequest.MemberIds);
        }
    }
}
