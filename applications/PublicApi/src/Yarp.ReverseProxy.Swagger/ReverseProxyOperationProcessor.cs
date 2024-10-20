using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
//using Yarp.ReverseProxy.Swagger.Extensions;
using Yarp.ReverseProxy.Transforms.Builder;

namespace Yarp.ReverseProxy.Swagger
{
    public class ReverseProxyOperationProcessor : IOperationProcessor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IReadOnlyDictionary<string, OperationType> _operationTypeMapping;
        private readonly List<ITransformFactory> _factories;
        
        private ReverseProxyDocumentFilterConfig config;

        public ReverseProxyOperationProcessor(
            //IServiceProvider serviceProviderIServiceProvider,
            IHttpClientFactory httpClientFactory,
            IOptionsMonitor<ReverseProxyDocumentFilterConfig> configOptions,
            IEnumerable<ITransformFactory> factories
            )
        {
            _factories = factories?.ToList();
            config = configOptions.CurrentValue;
            _httpClientFactory = httpClientFactory;

            configOptions.OnChange(x => { config = x; });

            _operationTypeMapping = new Dictionary<string, OperationType>
            {
                { "GET", OperationType.Get },
                { "POST", OperationType.Post },
                { "PUT", OperationType.Put },
                { "DELETE", OperationType.Delete },
                { "PATCH", OperationType.Patch },
                { "HEAD", OperationType.Head },
                { "OPTIONS", OperationType.Options },
                { "TRACE", OperationType.Trace },
            };
        }

        public bool Process(OperationProcessorContext context)
        {
            var operationDescription = context.OperationDescription;
            var swaggerDoc = context.Document;

            if (config.IsEmpty)
            {
                return false;
            }

            var clusters = config.Swagger.IsCommonDocument
                ? config.Clusters
                : config.Clusters.Where(x => x.Key == context.Document.BasePath) // Customized deviation
                    .ToDictionary(x => x.Key, x => x.Value);

            if (!clusters.Any())
            {
                return false;
            }

            foreach (var clusterKeyValuePair in clusters)
            {
                var clusterKey = clusterKeyValuePair.Key;
                var cluster = clusterKeyValuePair.Value;

                if (true != cluster.Destinations?.Any())
                {
                    continue;
                }

                foreach (var destination in cluster.Destinations)
                {
                    if (true != destination.Value.Swaggers?.Any())
                    {
                        continue;
                    }

                    var httpClient = _httpClientFactory.CreateClient($"{clusterKey}_{destination.Key}");

                    foreach (var swagger in destination.Value.Swaggers)
                    {
                        if (swagger.Paths?.Any() != true)
                        {
                            continue;
                        }

                        IReadOnlyDictionary<string, IEnumerable<string>> publishedRoutes = null;
                        if (swagger.AddOnlyPublishedPaths)
                        {
                            publishedRoutes = GetPublishedPaths(config);
                        }

                        Regex filterRegex = null;
                        if (!string.IsNullOrWhiteSpace(swagger.PathFilterRegexPattern))
                        {
                            filterRegex = new Regex(swagger.PathFilterRegexPattern);
                        }

                        foreach (var swaggerPath in swagger.Paths)
                        {
                            var stream = httpClient.GetStreamAsync($"{destination.Value.Address}{swaggerPath}").Result;
                            var doc = new OpenApiStreamReader().Read(stream, out _);

                            if (swagger.MetadataPath == swaggerPath)
                            {
                                //swaggerDoc.Info = doc.Info;
                                swaggerDoc.Info = ConvertToNSwagOpenApiInfo(doc.Info);
                            }

                            foreach (var path in doc.Paths)
                            {
                                var key = path.Key;
                                var value = path.Value;

                                if (filterRegex != null && !filterRegex.IsMatch(key))
                                {
                                    continue;
                                }

                                if (publishedRoutes != null)
                                {
                                    var pathKey = $"{swagger.PrefixPath}{path.Key}";
                                    if (!publishedRoutes.ContainsKey(pathKey))
                                    {
                                        continue;
                                    }

                                    var methods = publishedRoutes[pathKey];
                                    var operations = _operationTypeMapping
                                        .Where(q => methods.Contains(q.Key))
                                        .Select(q => q.Value)
                                        .ToList();

                                    if (!Enum.TryParse(context.OperationDescription.Method, true, out OperationType operationTypeParsed) || !operations.Contains(operationTypeParsed))
                                    {
                                        return false;
                                    }

                                    ApplySwaggerTransformation(operationTypeParsed, path, clusterKey);
                                }
                                else
                                {
                                    if (Enum.TryParse(context.OperationDescription.Method, true, out OperationType operationTypeParsed))
                                    {
                                        ApplySwaggerTransformation(operationTypeParsed, path, clusterKey);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }

        private static IReadOnlyDictionary<string, IEnumerable<string>> GetPublishedPaths(
            ReverseProxyDocumentFilterConfig config)
        {
            var validRoutes = new Dictionary<string, IEnumerable<string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var route in config.Routes)
            {
                if (route.Value?.Match.Path == null)
                {
                    continue;
                }

                if (!validRoutes.ContainsKey(route.Value.Match.Path))
                {
                    if (route.Value.Transforms != null)
                    {
                        foreach (var transform in route.Value.Transforms)
                        {
                            foreach (var (key, value) in transform)
                            {
                                validRoutes.TryAdd(value, route.Value.Match.Methods);
                            }
                        }
                    }
                }

                if (!validRoutes.ContainsKey(route.Value.Match.Path))
                {
                    validRoutes.TryAdd(route.Value.Match.Path, route.Value.Match.Methods);
                }
                else
                {
                    if (route.Value.Match.Methods != null)
                    {
                        validRoutes[route.Value.Match.Path] =
                            validRoutes[route.Value.Match.Path].Concat(route.Value.Match.Methods);
                    }
                }
            }

            return validRoutes;
        }

        private void ApplySwaggerTransformation(OperationType operationKey,
            KeyValuePair<string, OpenApiPathItem> path, string clusterKey)
        {
            var factories = _factories?.Where(x => x is ISwaggerTransformFactory).ToList();

            if (factories == null) return;

            path.Value.Operations.TryGetValue(operationKey, out var operation);

            var transforms = config.Routes
                .Where(x => x.Value.ClusterId == clusterKey)
                .Where(x => x.Value.Transforms != null)
                .SelectMany(x => x.Value.Transforms)
                .ToList();

            foreach (var swaggerFactory in factories.Select(factory => factory as ISwaggerTransformFactory))
            {
                foreach (var transform in transforms)
                {
                    swaggerFactory?.Build(operation, transform);
                }
            }
        }

        private NSwag.OpenApiInfo ConvertToNSwagOpenApiInfo(OpenApiInfo openApiInfo)
        {
            return new NSwag.OpenApiInfo
            {
                Title = openApiInfo.Title,
                Description = openApiInfo.Description,
                Version = openApiInfo.Version,
                TermsOfService = openApiInfo.TermsOfService?.ToString(),
                //Contact = openApiInfo.Contact != null ? new OpenApiContact
                //{
                //    Name = openApiInfo.Contact.Name,
                //    Email = openApiInfo.Contact.Email,
                //    Url = openApiInfo.Contact.Url
                //} : null,
                //License = openApiInfo.License != null ? new OpenApiLicense
                //{
                //    Name = openApiInfo.License.Name,
                //    Url = openApiInfo.License.Url?.ToString()
                //} : null
            };
        }

    }
}
