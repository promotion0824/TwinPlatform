using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Willow.DataQuality.Model.Responses;
using Willow.DataQuality.Model.Validation;
using Willow.Model.Responses;

namespace Willow.AzureDigitalTwins.Api.Extensions
{
    public static class ApiExtensions
    {
        public static Azure.JsonPatchDocument ToAzureJsonPatchDocument<T>(this JsonPatchDocument<T> jsonPatchDocument)
            where T : class
        {
            var azureJsonPatchDocument = new Azure.JsonPatchDocument();

            var actionMap = new Dictionary<OperationType, Action<Operation<T>>>
            {
                { OperationType.Add, x =>
                    {
                        if(x.value is JArray || x.value is JObject)
                        {
                            azureJsonPatchDocument.AppendAddRaw(x.path, Convert.ToString(x.value));
                        }
                        else
                        {
                            azureJsonPatchDocument.AppendAdd(x.path,x.value);
                        }
                    }
                },
                { OperationType.Copy, x => azureJsonPatchDocument.AppendCopy(x.from, x.path) },
                { OperationType.Move, x => azureJsonPatchDocument.AppendMove(x.from, x.path) },
                { OperationType.Remove, x => azureJsonPatchDocument.AppendRemove(x.path) },
                { OperationType.Replace, x =>
                    {
                        if(x.value is JArray || x.value is JObject)
                        {
                            azureJsonPatchDocument.AppendReplaceRaw(x.path, Convert.ToString(x.value));
                        }
                        else
                        {
                            azureJsonPatchDocument.AppendReplace(x.path,x.value);
                        }
                    }
                },
                { OperationType.Test, x =>
                    {
                        if(x.value is JArray || x.value is JObject)
                        {
                            azureJsonPatchDocument.AppendTestRaw(x.path, Convert.ToString(x.value));
                        }
                        else
                        {
                            azureJsonPatchDocument.AppendTest(x.path,x.value);
                        }
                    }
                }
            };

            jsonPatchDocument.Operations.ForEach(op =>
            {
                op.path = op.path?.Replace("/customproperties", string.Empty);
                op.from = op.from?.Replace("/customproperties", string.Empty);

                actionMap[op.OperationType](op);
            });

            return azureJsonPatchDocument;
        }

        public static string ToJsonString(this JsonDocument jdoc)
        {
            using (var stream = new MemoryStream())
            {
                Utf8JsonWriter writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });
                jdoc.WriteTo(writer);
                writer.Flush();
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        public static IEnumerable<TwinValidationResultResponse> ToApiValidationResults(
            this IEnumerable<RuleTemplateValidationResult> validationResults)
        {
            return validationResults
                .Where(x => !x.IsValid)
                .GroupBy(x => x.TwinWithRelationship.Twin.Id)
                .Select(x => new TwinValidationResultResponse
                {
                    TwinId = x.Key,
                    Results = x.GroupBy(v => v.RuleTemplate.Id).Select(r => new ValidationRuleResult
                    {
                        RuleId = r.Key,
                        PropertyErrors = r.SelectMany(y => y.PropertyValidationResults)
                            .GroupBy(p => p.propertyName)
                            .ToDictionary(p => p.Key, p => p.Select(y => y.type))
                    })
                });
        }

        public static MultipleEntityResponse Add(
            this MultipleEntityResponse self, HttpStatusCode status,
            string entityId, string subEntityId = null,
            string operation = null, string msg = null)
        {
            self.Responses.Add(new EntityResponse
            {
                StatusCode = status,
                EntityId = entityId,
                SubEntityId = subEntityId,
                Message = msg
            });
            return self;
        }
        public static MultipleEntityResponse Add(
            this MultipleEntityResponse self, HttpStatusCode status,
            string entityId, string subEntityId = null,
            string operation = null, Exception ex = null)
        {
            var msg = JsonSerializer.Serialize(ex.Message);
            return self.Add(status, entityId, subEntityId, operation, msg);
        }

        // This overload is to avoid writing another version for non-generic Task because we can't do a Task<void> in C# 
        public static async Task<bool> ExecuteAsync<TL>(
            this MultipleEntityResponse self,
            string id,
            Func<Task> fn,
            string operation = null,
            string subId = null,
            ILogger<TL> logger = null,
            bool onlyAddErrors = false)
        {
            var (ok, _) = await ExecuteAsync(self, id,
                async () => { await fn(); return ""; },
                operation, subId, logger, onlyAddErrors);
            return ok;
        }

        public static async Task<(bool, T)> ExecuteAsync<T, TL>(
            this MultipleEntityResponse self,
            string id,
            Func<Task<T>> fn,
            string operation = null,
            string subId = null,
            ILogger<TL> logger = null,
            bool onlyAddErrors = false) where T : class
        {
            try
            {
                T entity = await fn.Invoke();
                if (!onlyAddErrors)
                    self.Add(HttpStatusCode.OK, id, subId, operation, msg: null);
                return (true, entity);
            }
            catch (HttpRequestException ex)
            {
                logger?.LogError("ExecuteAsync: RequestException for operation {op}", ex);
                self.Add(ex.StatusCode ?? HttpStatusCode.InternalServerError, id, subId, operation, ex);
                return (false, null);
            }
            catch (Exception ex)
            {
                logger?.LogError("ExecuteAsync: Exception for operation {op}", ex);
                self.Add(HttpStatusCode.InternalServerError, id, subId, operation, ex);
                return (false, null);
            }
        }
    }
}
