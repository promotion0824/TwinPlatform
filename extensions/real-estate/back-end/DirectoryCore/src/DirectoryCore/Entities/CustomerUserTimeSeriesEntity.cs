using System;
using System.Text.Json;
using DirectoryCore.Dto;
using Willow.Infrastructure;

namespace DirectoryCore.Entities
{
    public class CustomerUserTimeSeriesEntity
    {
        public Guid CustomerUserId { get; set; }
        public string State { get; set; }
        public string Favorites { get; set; }
        public string RecentAssets { get; set; }
        public string ExportedCsvs { get; set; }
        public UserEntity User { get; set; }

        public static CustomerUserTimeSeriesDto MapTo(CustomerUserTimeSeriesEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            var customerUserTimeSeriesDto = new CustomerUserTimeSeriesDto();
            customerUserTimeSeriesDto.State = Deserialize(entity.State, "{}");
            customerUserTimeSeriesDto.Favorites = Deserialize(entity.Favorites, "[]");
            customerUserTimeSeriesDto.RecentAssets = Deserialize(entity.RecentAssets, "{}");
            customerUserTimeSeriesDto.ExportedCsvs = Deserialize(entity.ExportedCsvs, "[]");

            return customerUserTimeSeriesDto;
        }

        private static JsonElement Deserialize(string field, string defaultValue)
        {
            return field != null
                ? JsonSerializerExtensions.Deserialize<JsonElement>(field)
                : JsonSerializerExtensions.Deserialize<JsonElement>(defaultValue);
        }
    }
}
