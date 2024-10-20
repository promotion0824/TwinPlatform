using System.Threading.Tasks;
using InsightCore.Dto;
using InsightCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Willow.Batch;

namespace InsightCore.Controllers
{
    [ApiController]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [Produces("application/json")]
    public class SkillsController : ControllerBase
    {
        private readonly ISkillService _skillsService;
        public SkillsController(ISkillService skillsService)
        {
            _skillsService = skillsService; 
        }

        [HttpPost("skills")]
        [Authorize]
        public async Task<ActionResult<BatchDto<SkillDto>>> GetSkills([FromBody] BatchRequestDto request)
        {
            return await _skillsService.GetSkillsAsync(request,true);
           
        }

    }
}
