using System.Threading.Tasks;
using WorkflowCore.Repository;

namespace WorkflowCore.Services
{
    /// <summary>
    /// This service was part of WorkflowService.
    ///
    /// With the update to net7 there was a breaking change with how provider extensions
    /// handle raw sql queries. InMemoryDb has no support for it.
    ///
    /// This service allows the funtionality to be mocked during unit testing. 
    /// </summary>
    
    public interface IWorkflowSequenceNumberService
    {
        Task<string> GenerateSequenceNumber(string prefix, string key = "S");
    }

    public class WorkflowSequenceNumberService : IWorkflowSequenceNumberService
    {
        private readonly IWorkflowRepository _repository;

        public WorkflowSequenceNumberService(IWorkflowRepository repository)
        {
            _repository = repository;
        }

        public async Task<string> GenerateSequenceNumber(string prefix, string key = "S")
        {
            var number = await _repository.GenerateSequenceNumber(prefix, key);

            return $"{prefix}-{key}-{number}";
        }
    }
}
