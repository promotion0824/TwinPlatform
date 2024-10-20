using System;
using System.Text.Json;
using DirectoryCore.Domain;
using DirectoryCore.Enums;
using Willow.Infrastructure;

namespace DirectoryCore.Entities
{
    public class CustomerUserPreferencesEntity
    {
        public Guid CustomerUserId { get; set; }
        public bool MobileNotificationEnabled { get; set; }
        public string Language { get; set; }
        public string Profile { get; set; }
        public UserEntity User { get; set; }

        public static CustomerUserPreferences MapTo(CustomerUserPreferencesEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var customerUserPreferences = new CustomerUserPreferences();
            customerUserPreferences.MobileNotificationEnabled = entity.MobileNotificationEnabled;
            customerUserPreferences.Language = entity.Language;

            if (string.IsNullOrEmpty(entity.Profile))
            {
                customerUserPreferences.Profile = JsonSerializerExtensions.Deserialize<JsonElement>(
                    "{}"
                );

                return customerUserPreferences;
            }

            customerUserPreferences.Profile = JsonSerializerExtensions.Deserialize<JsonElement>(
                entity.Profile
            );

            return customerUserPreferences;
        }
    }
}
