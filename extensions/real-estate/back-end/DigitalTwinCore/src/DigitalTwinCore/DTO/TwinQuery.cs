using DigitalTwinCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DigitalTwinCore.Dto
{
    [Serializable]
    // Incoming body parameter for an ADT twin query
    public class TwinAdtSqlQuery
    {
        // ADT SQL pass-thru query for dev/debug use only - do not expose to public API
        public string Query { get; set; }
        // TODO: could add "nocache" option here -- currently FromQuery
        // TODO: Currently all endpoints require /siteId/ base, but the 
        //   query endpoints will operate across all sites in an ADT instance,
        //   unless the query itself filters based on siteId.
        //   We could a list of siteIds here (default [] means all sites) to limit the scope to
    }


    [Serializable]
    public class RelationshipQuery
    {
        public string[] FollowAny { get; set; }
        public string ToModel { get; set; }
    }

    [Serializable]
    // Base-class DTO for limited query
    public class TwinSimpleQueryBase
    {
        public bool FromCache { get; set; } = true;
        // Only return twins that have a reachable Site that matches the site in the query path
        public bool RestrictToSite { get; set; } = true;
        // Only return parent item if children is non-empty
        public bool RestrictWithChildren { get; set; } = false;
        public string[] RootModels { get; set; }
        public RelationshipQuery Relationships { get; set; }
        // TODO: Add general property matching
        //   ["", "isOfModel", "Document"]   ["geometyViewerId", "eq", "..."]  ["uniqueId", "eqOneOf", [..., ...]]

    }

    [Serializable]
    // Derived class for RealEstate query
    public class TwinSimpleRealEstateQuery : TwinSimpleQueryBase
    {
        public string[] Floors { get; set; }
        // TODO: Add K/V pairs here for property matching
    }
}
