using DigitalTwinCore.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using DigitalTwinCore.Constants;
using DigitalTwinCore.Extensions;
using DigitalTwinCore.Exceptions;
using Azure.DigitalTwins.Core;
using Azure;

namespace DigitalTwinCore.Models
{
    [Serializable]
    public class Twin
    {
        private static readonly MD5 Md5Hasher = MD5.Create();
        public string Id { get; set; }
        public TwinMetadata Metadata { get; set; }
        public string Etag { get; set; }
        public IDictionary<string, object> CustomProperties { get; set; }
        public string DisplayName => this.GetStringProperty(Properties.Name) ?? Id;

        private Guid? _uniqueId = null;

        private NullableNullable<Guid> _siteID;
        // If we find that a twin has no siteID, keep it null and don't try to look it up again
        public Guid? GetSiteId(string[] siteModelIds = null) =>
            _siteID.HasValue ? _siteID.Value : (_siteID.Value = this.FindSiteId(siteModelIds));
        public string GetSiteIdString(string[] siteMdelIds = null) =>
            GetSiteId(siteMdelIds).HasValue ? _siteID.Value.ToString() : null;

        public string ExternalId =>  this.GetStringProperty(Properties.ExternalID);
        public string TrendId =>  this.GetStringProperty(Properties.TrendID);

        // We want to rely of DigitalTwinService to assign any missing uniqueIds and trap any cases where we have not done so.
        // public Guid UniqueId => _uniqueId ??=  UniqueIdFromProperties ??  ConvertStringToGuid(Id);
        public Guid UniqueId
        {
            get =>
                _uniqueId ??=
                    UniqueIdFromProperties ??
                    throw new DigitalTwinCoreException(GetSiteId(), $"No UniqueId found for twin: {Id}");
            set => _uniqueId = value;
        }

        public Guid? UniqueIdFromProperties => TryGetGuid(Properties.UniqueId);

        #region ADX Export
        public bool Deleted { get; internal set; }
        public string ModelId { get; internal set; }
        public DateTime ExportTime => DateTime.UtcNow;
		public Guid? SiteId { get; internal set; }
        public Guid? FloorId { get; internal set; }
        public Guid? ConnectorId 
        {
            get
            {
                return TryGetGuid(Properties.ConnectorID);
            }
        }
        public Guid? GeometryViewerId
        {
            get
            {
                return TryGetGuid(Properties.GeometryViewerId);
            }
        }

        public Guid? TryGetGuid(string key)
        {
            return CustomProperties?.GetValueOrDefault(key) switch
            {
                string propertyString => Guid.TryParse(propertyString, out Guid id) ? id : (Guid?)null,
                Guid propertyGuid => propertyGuid,
                JsonElement propertyElement => Guid.TryParse(propertyElement.GetString(), out Guid id) ? id : (Guid?)null,
                _ => null
            };
        }

        public string Tags
        {
            get
            {
                var tags = CustomProperties?.GetValueOrDefault(Properties.Tags);
                if (tags == null)
                    return null;

                var tagNames = ((Dictionary<string, object>)tags)
                    .Select(p => p.Key);

                return string.Join(',', tagNames);
            }
        }
		#endregion

		public static IDictionary<string, object> MapCustomProperties(IDictionary<string, object> customProperties)
        {
            return customProperties.AsEnumerable().ToDictionary(
                kv => kv.Key,
                kv => kv.Value switch
                {
                    JsonElement element => element.ToObject(),
                    _ => kv.Value
                });
        }

        internal static Twin MapFrom(BasicDigitalTwin dto)
        {
            return new Twin
            {
                CustomProperties = MapCustomProperties(dto.Contents),
                Id = dto.Id,
                Metadata = TwinMetadata.MapFrom(dto.Metadata)
            };
        }

        internal static IEnumerable<Twin> MapFrom(IEnumerable<BasicDigitalTwin> dtos)
        {
            return dtos.Select(MapFrom);
        }

        public static Twin MapFrom(TwinDto dto)
        {
            return new Twin
            {
                CustomProperties = dto.CustomProperties,
                Id = dto.Id,
                Metadata = TwinMetadata.MapFrom(dto.Metadata),
                Etag = dto.Etag
            };
        }

        public BasicDigitalTwin MapToDto()
        {
            return new BasicDigitalTwin
            {
                Contents = CustomProperties,
                Id = Id,
                Metadata = new DigitalTwinMetadata
                {
                    ModelId = Metadata.ModelId,
                }
            };
        }
    }
}
