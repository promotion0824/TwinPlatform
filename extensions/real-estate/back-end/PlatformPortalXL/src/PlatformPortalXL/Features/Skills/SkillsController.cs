using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.ServicesApi.InsightApi;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using PlatformPortalXL.Extensions;
using Willow.Batch;

namespace PlatformPortalXL.Features.Skills;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class SkillsController : ControllerBase
{
    private readonly ILogger<SkillsController> _logger;
    private readonly IInsightApiService _insightApi;
       
    public SkillsController(
        ILogger<SkillsController> logger,
        IInsightApiService insightApi)
    {
        _logger = logger;
        _insightApi = insightApi;
    }

    [HttpPost("skills")]
    [Authorize]
    [ProducesResponseType(typeof(BatchDto<SkillDto>), StatusCodes.Status200OK)]
    [SwaggerOperation("Gets a batch of skills", Tags = new[] { "Skills" })]
    public async Task<ActionResult> GetSkillsAsync([FromBody] BatchRequestDto request)
    {
       var skills= await _insightApi.GetSkills(request);

        return Ok(skills);
    }

    [HttpGet("skills/categories")]
    [Authorize]
    [ProducesResponseType(typeof(List<EnumKeyValueDto>), StatusCodes.Status200OK)]
    [SwaggerOperation("Gets a list of all available skill's categories", Tags = new[] { "Skills" })]
    public ActionResult GetSkillCategoriesAsync()
    {
        return Ok(typeof(SkillCategory).ToEnumKeyValueDto());
    }

}
