using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Willow.AzureDigitalTwins.SDK.Client;
using Willow.TwinLifecycleManagement.Web.Auth;

namespace Willow.TwinLifecycleManagement.Web.Controllers;

/// <summary>
/// Search Controller.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SearchController"/> class.
/// </remarks>
/// <param name="searchClient">Search Client.</param>
[Route("api/[controller]")]
[ApiController]
public class SearchController(ISearchClient searchClient) : ControllerBase
{
    /// <summary>
    /// Search Document Index.
    /// </summary>
    /// <param name="searchQuery">Search query.</param>
    /// <param name="mode">Search mode. [Keyword, Vector, Hybrid].</param>
    /// <param name="skip">Number of results to skip.</param>
    /// <param name="take">Number of results to take.</param>
    /// <returns>DocumentSearchResponse.</returns>
    [HttpGet("Document")]
    [Authorize(Policy = AppPermissions.CanSearchDocuments)]
    public async Task<DocumentSearchResponse> Document(string searchQuery, DocumentSearchMode mode, int skip, int take)
    {
        return await searchClient.QueryDocumentIndexAsync(searchQuery, mode, skip, take);
    }
}
