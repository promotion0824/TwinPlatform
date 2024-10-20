using Authorization.Common.Enums;
using Authorization.TwinPlatform.Abstracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authorization.TwinPlatform.Permission.Api.Controllers;

[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("api/[controller]")]
[ApiController]
public class CacheController(ICacheInvalidationService cacheInvalidationService) : ControllerBase
{

    /// <summary>
    /// Reset the cache.
    /// </summary>
    /// <param name="cacheStoreTypes">Array of <typeparamref name="CacheStoreType"/>.</param>
    /// <returns> <typeparamref name="ActionResult"/> <returns>
    [HttpPost("invalidate")]
    public ActionResult InvalidateCache([FromBody] CacheStoreType[] cacheStoreTypes)
    {
        if (cacheStoreTypes == null || cacheStoreTypes.Length == 0)
        {
            return ValidationProblem("Cache store type required for cache invalidation.");
        }

        foreach(var cacheStoreType in cacheStoreTypes)
        {
            cacheInvalidationService.InvalidateCache(cacheStoreType);
        }

        return Ok();
    }
}
