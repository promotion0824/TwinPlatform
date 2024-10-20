using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System;
using MobileXL.Dto;
using MobileXL.Features.Insight.Requests;
using MobileXL.Security;
using MobileXL.Services;
using MobileXL.Services.Apis.InsightApi;

namespace MobileXL.Features.Insight;

[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class InsightsController : ControllerBase
{
	private readonly IAccessControlService _accessControl;
	private readonly IInsightApiService _insightApi;
	public InsightsController(IAccessControlService accessControl, IInsightApiService insightApi)
	{
		_accessControl = accessControl;
		_insightApi=insightApi;
	}

	/// <summary>
	/// Updates the status of the given insight
	/// </summary>
	/// <param name="siteId">the site id for the insight</param>
	/// <param name="insightId">the id for the insight</param>
	/// <param name="request"> the requested status for the insight</param>
	/// <returns></returns>
	[HttpPut("sites/{siteId}/insights/{insightId}/status")]
	[MobileAuthorize]
	[ProducesResponseType(typeof(InsightDetailDto), StatusCodes.Status200OK)]
	[SwaggerOperation("Updates the status of the given insight", Tags = new[] { "Insights" })]
	public async Task<ActionResult> UpdateInsightStatus([FromRoute] Guid siteId, [FromRoute] Guid insightId, [FromBody] UpdateInsightStatusRequest request)
	{
		var userId = this.GetCurrentUserId();
		await _accessControl.EnsureAccessSite(this.GetCurrentUserType(), userId, siteId);

		var insight = await _insightApi.UpdateInsightAsync(siteId, insightId, userId, status: request.Status.Value);
		return Ok(InsightDetailDto.MapFromModel(insight));
	}
}
