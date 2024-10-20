using DigitalTwinCore.Models;
using DigitalTwinCore.Services.AdtApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.JsonPatch;
using Willow.Infrastructure.Exceptions;
using Azure.DigitalTwins.Core;
using System.Threading.Tasks;
using DigitalTwinCore.Services.Cacheless;

namespace DigitalTwinCore.Test.MockServices
{
    public class InMemoryAdtApiService : IAdtApiService
    {
        private readonly Dictionary<string, AdtModel> _models = new Dictionary<string, AdtModel>();
        private readonly Dictionary<string, Dictionary<string, BasicRelationship>> _relationships = new Dictionary<string, Dictionary<string, BasicRelationship>>();
        private readonly Dictionary<string, BasicDigitalTwin> _twins = new Dictionary<string, BasicDigitalTwin>();

        public Task<BasicDigitalTwin> AddOrUpdateTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, BasicDigitalTwin twin)
        {
            if (twinId == null)
            {
                twinId = twin.Id;
            }
            _twins[twinId] = twin;
            return Task.FromResult(twin);
        }

        public Task<BasicRelationship> AddRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, BasicRelationship relationship)
        {
            if (!_relationships.ContainsKey(twinId))
            {
                _relationships[twinId] = new Dictionary<string, BasicRelationship>();
            }
            _relationships[twinId][relationshipId] = relationship;
            return Task.FromResult(relationship);
        }

        // Return "str" for either "k":"str"  or "k":{"en":str}, otherwise null
        // We could also convert any inner Object into the DisplayNames dictionary,
        //   but this is easier as we don't need arbitrary languages here and we don't want to have to validate them.
        private string getPossibleLocalizedStringProperty(JsonElement e) => e.ValueKind switch
            {
                JsonValueKind.String => e.GetString(),
                JsonValueKind.Object => e.TryGetProperty("en", out var prop) ? prop.GetString() : null,
                _ => null
            };

        public Task<AdtModel> CreateModel(AzureDigitalTwinsSettings instanceSettings, string modelJson)
        {
            using var jsonDocument = JsonDocument.Parse(modelJson);
            var hasDisplayName = jsonDocument.RootElement.TryGetProperty("displayName", out var prop);
            var displayName = hasDisplayName ? getPossibleLocalizedStringProperty(prop) : null;
            var id = jsonDocument.RootElement.GetProperty("@id").GetString();

            var adtModel = new AdtModel
            {
                DisplayNames = displayName == null ? null : new Dictionary<string, string> {{"en", displayName}},
                Id = id,
                Model = modelJson,
            };

            _models[adtModel.Id] = adtModel;
            return Task.FromResult(adtModel);
        }



        public Task DeleteModel(AzureDigitalTwinsSettings instanceSettings, string modelId)
        {
            _models.Remove(modelId);
            return Task.CompletedTask;
        }

        public Task DeleteRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId)
        {
            try
            {
                _relationships[twinId].Remove(relationshipId);
            }
            catch (Exception)
            {
                throw new ResourceNotFoundException("Relationship", relationshipId);
            }

            return Task.CompletedTask;
        }

        public Task DeleteTwin(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            if (_twins.ContainsKey(twinId))
            {
                _twins.Remove(twinId);
            }
            else
            {
                throw new ResourceNotFoundException("Twin", twinId);
            }

            return Task.CompletedTask;
        }

        public Task<List<Azure.DigitalTwins.Core.IncomingRelationship>> GetIncomingRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            throw new NotImplementedException();
        }

        public Task<AdtModel> GetModel(AzureDigitalTwinsSettings instanceSettings, string modelId)
        {
            return Task.FromResult(_models[modelId]);
        }

        public List<AdtModel> GetModels(AzureDigitalTwinsSettings instanceSettings)
        {
            return _models.Values.ToList();
        }

        public Task<BasicRelationship> GetRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId)
        {
            return Task.FromResult(_relationships[twinId][relationshipId]);
        }

        public Task<List<BasicRelationship>> GetRelationships(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            if (_relationships.ContainsKey(twinId))
            {
                return Task.FromResult(_relationships[twinId].Values.ToList());
            }
            return Task.FromResult(new List<BasicRelationship>());
        }

        public Task<BasicDigitalTwin> GetTwin(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            return Task.FromResult(_twins[twinId]);
        }

        public Task<List<BasicDigitalTwin>> GetTwins(AzureDigitalTwinsSettings instanceSettings, string query = null)
        {
            return Task.FromResult(_twins.Values.ToList());
        }

        public Task<List<JsonElement>> QueryTwins(AzureDigitalTwinsSettings instanceSettings, string sql)
        {
            throw new NotImplementedException();
        }

        public Task<BasicRelationship> UpdateRelationship(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipId, JsonPatchDocument patchJson)
        {
            throw new NotImplementedException();
        }

        public Task PatchTwin(AzureDigitalTwinsSettings instanceSettings, string twinId, JsonPatchDocument patch, Azure.ETag? ifMatch)
        {
            throw new NotImplementedException();
        }

        public Azure.AsyncPageable<T> QueryTwins<T>(AzureDigitalTwinsSettings instanceSettings, string sql)
        {
            throw new NotImplementedException();
        }

        Dictionary<string, List<string>> IAdtApiService.GetLatestExecutedQueries()
        {
            throw new NotImplementedException();
        }

        public Task<Page<BasicDigitalTwin>> GetTwinsAsync(AzureDigitalTwinsSettings instanceSettings, GetTwinsInfoRequest request = null, IEnumerable<string> twinIds = null, int pageSize = 100, bool includeCountQuery = false, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BasicRelationship>> GetRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId, string relationshipName = null)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<BasicRelationship>> GetIncomingRelationshipsAsync(AzureDigitalTwinsSettings instanceSettings, string twinId)
        {
            throw new NotImplementedException();
        }
    }
}
