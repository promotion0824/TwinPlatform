using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Repository;
using WorkflowCore.Models;
using WorkflowCore.Controllers.Request;
using Willow.Infrastructure.Exceptions;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
namespace WorkflowCore.Services
{
    public interface IReportersService
    {
        Task<List<Reporter>> GetReporters(Guid siteId);
        Task<Reporter> CreateReporter(Guid siteId, CreateReporterRequest request);
        Task<Reporter> UpdateReporter(Guid siteId, Guid reporterId, UpdateReporterRequest request);
        Task DeleteReporter(Guid siteId, Guid reporterId);
    }

    public class ReportersService : IReportersService
    {
        private readonly IWorkflowRepository _repository;

        public ReportersService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Reporter>> GetReporters(Guid siteId)
        {
            return await _repository.GetReporters(siteId);
        }

        public async Task<Reporter> CreateReporter(Guid siteId, CreateReporterRequest request)
        {
            var reporter = new Reporter
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                SiteId = siteId,
                Name = request.Name,
                Phone = request.Phone,
                Email = request.Email,
                Company = request.Company
            };
            await _repository.CreateReporter(reporter);
            return reporter;
        }

        public async Task<Reporter> UpdateReporter(Guid siteId, Guid reporterId, UpdateReporterRequest request)
        {
            var reporter = await _repository.GetReporter(siteId, reporterId);
            if (reporter == null)
            {
                throw new NotFoundException(new { ReporterId = reporterId });
            }
            reporter.Name = request.Name;
            reporter.Phone = request.Phone;
            reporter.Email = request.Email;
            reporter.Company = request.Company;
            await _repository.UpdateReporter(reporter);
            return reporter;
        }

        public async Task DeleteReporter(Guid siteId, Guid reporterId)
        {
            var deleted = await _repository.DeleteReporter(siteId, reporterId);
            if (!deleted)
            {
                throw new NotFoundException(new { ReporterId = reporterId });
            }
        }

    }
}
