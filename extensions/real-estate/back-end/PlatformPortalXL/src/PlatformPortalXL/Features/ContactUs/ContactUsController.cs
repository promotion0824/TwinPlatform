using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PlatformPortalXL.Extensions;
using Microsoft.AspNetCore.Http;
using PlatformPortalXL.Dto;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;
using PlatformPortalXL.Services.ContactUs;
using Swashbuckle.AspNetCore.Annotations;
using Willow.Api.DataValidation;
using Willow.Common;
using Willow.ExceptionHandling.Exceptions;
using PlatformPortalXL.Auth.Services;

namespace PlatformPortalXL.Features.ContactUs;

[Route("[controller]")]
[ApiController]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
public class ContactUsController : ControllerBase
{
    private readonly ILogger<ContactUsController> _logger;
    private readonly IContactUsService _contactUsService;
    private readonly IAccessControlService _accessControl;
    private readonly IUserAuthorizedSitesService _userAuthorizedSitesService;
    public ContactUsController(ILogger<ContactUsController> logger,
                               IContactUsService contactUsService,
                               IAccessControlService accessControl,
                               IUserAuthorizedSitesService userAuthorizedSitesService)
    {
        _logger = logger;
        _contactUsService = contactUsService;
        _accessControl = accessControl;
        _userAuthorizedSitesService = userAuthorizedSitesService;
    }

    /// <summary>
    /// List of support's categories
    /// </summary>
    /// <returns>Returns the support's category enum items</returns>
    [Authorize]
    [HttpGet("categories")]
    [ProducesResponseType(typeof(List<EnumKeyValueDto>), StatusCodes.Status200OK)]
    public IActionResult GetSupportedCategories()
    {
        return Ok(typeof(ContactUsCategory).ToEnumKeyValueDto());
    }

    /// <summary>
    /// Create a support ticket
    /// </summary>
    /// <param name="request">support ticket request</param>
    /// <param name="attachmentFiles">list of attachments</param>
    /// <returns>201</returns>
    /// 

    [Consumes("multipart/form-data")]
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ValidationError), StatusCodes.Status422UnprocessableEntity)]
    [SwaggerOperation("Create a support Ticket", Tags = new[] { "ContactUs" })]
    public async Task<IActionResult> CreateSupportTicket([FromForm] CreateSupportTicketRequest request, [FromForm] IFormFileCollection attachmentFiles)
    {
        var currentUserId = this.GetCurrentUserId();
        if(request.SiteId.HasValue)
            await _accessControl.EnsureAccessSite(currentUserId, Permissions.ViewSites, request.SiteId.Value);

        var site = (await _userAuthorizedSitesService.GetAuthorizedSites(currentUserId, Permissions.ViewSites))
            .FirstOrDefault(x => !request.SiteId.HasValue || request.SiteId.Value == x.Id);
        if (site == null)
        {
            throw new NotFoundException().WithData(new { request.SiteId });
        }
        await _contactUsService.CreateSupportTicket(request, attachmentFiles,site,currentUserId);
        return StatusCode(StatusCodes.Status201Created);
    }

}

