using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SiteCore.Domain;
using Willow.Infrastructure;

namespace SiteCore.Entities
{
    [Table("SitePreferences")]
    public class SitePreferencesEntity
    {
        public Guid SiteId { get; set; }
        public string TimeMachine { get; set; }
        public string ModuleGroups { get; set; }
        public string ScopeId { get; set; }

        public static SitePreferences MapTo(SitePreferencesEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var sitePreferences = new SitePreferences();
            if (string.IsNullOrEmpty(entity.TimeMachine))
            {
                sitePreferences.TimeMachine = JsonSerializerExtensions.Deserialize<JsonElement>("{\"favorites\": []}");
            }
            else
            {
                sitePreferences.TimeMachine = JsonSerializerExtensions.Deserialize<JsonElement>(entity.TimeMachine);
            }
            
            if (string.IsNullOrEmpty(entity.ModuleGroups))
            {
                sitePreferences.ModuleGroups = JsonSerializerExtensions.Deserialize<JsonElement>("{}");
            }
            else
            {
                sitePreferences.ModuleGroups = JsonSerializerExtensions.Deserialize<JsonElement>(entity.ModuleGroups);
            }

            return sitePreferences;
        }
    }
}
