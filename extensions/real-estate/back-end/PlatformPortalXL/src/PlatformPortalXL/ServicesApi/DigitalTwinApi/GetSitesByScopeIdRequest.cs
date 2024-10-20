using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using Willow.Common;
using Willow.Platform.Models;

namespace PlatformPortalXL.Features.Scopes
{
    public class GetSitesByScopeIdRequest
    {
        public ScopeIdRequest Scope { get; set; }
    }

    public class ScopeIdRequest
    {
        [Required]
        public string DtId { get; set; }

        public List<Guid> SiteIds
        {
            get
            {
                return UserSites?.Select(x => x.Id).ToList();
            }
        }

        [JsonIgnore]
        [Required]
        public List<Site> UserSites { get; set; }

        [JsonIgnore]
        public string CacheKey
        {
            get
            {
                return $"{nameof(ScopeIdRequest)}_{DtId}_{SiteIds.OrderBy(x => x).FirstOrDefault() }";
            }
        }
    }
}
