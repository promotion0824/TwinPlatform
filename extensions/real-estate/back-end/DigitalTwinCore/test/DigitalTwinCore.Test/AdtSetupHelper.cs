using DigitalTwinCore.Models;
using DigitalTwinCore.Test.MockServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using DigitalTwinCore.Constants;

namespace DigitalTwinCore.Test
{
    public class AdtSetupHelper
    {
        private readonly TestDigitalTwinService _digitalTwinService;

        public AdtSetupHelper(TestDigitalTwinService digitalTwinService)
        {
            _digitalTwinService = digitalTwinService;
        }

        private InMemoryAdtApiService AdtApiService => _digitalTwinService.AdtApiService as InMemoryAdtApiService;

        private AzureDigitalTwinsSettings AdtSettings => _digitalTwinService.SiteAdtSettings.InstanceSettings;


        public void SetupModels()
        {
            AddSharedModels();
            AddModels("BuildingSpecific", "GenericSite.json");
            AddModels("OtherBuildingSpecific", "GenericSite.json");
        }

        public void SetupTwins(string siteCode)
        {
            SetupTwins(siteCode, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        }

        public void SetupTwins(string siteCode, Guid siteId)
        {
            SetupTwins(siteCode, siteId, Guid.NewGuid(), Guid.NewGuid());
        }

        public string SetupTwins(string siteCode, Guid siteId, Guid floor1Id, Guid floor2Id)
        {
            var siteTwinId = AddTwin(CreateTwin("Land", "Site1", siteId));

            var buildingTwinId = AddTwin(CreateTwin("Building", "Building1", siteId, siteId));
            AddRelationship(buildingTwinId, siteTwinId, Relationships.IsPartOf);

            var level1TwinId = AddTwin(CreateTwin("Level", "Level1", siteId, floor1Id));
            AddRelationship(level1TwinId, buildingTwinId, Relationships.IsPartOf);

            var level2TwinId = AddTwin(CreateTwin("Level", "Level2", siteId, floor2Id));
            AddRelationship(level2TwinId, buildingTwinId, Relationships.IsPartOf);

            var ahu1TwinId = AddTwin(CreateTwinWithSiteCode(siteCode, "AirHandlingUnit", "AHU1", siteId));
            AddRelationship(ahu1TwinId, buildingTwinId, Relationships.LocatedIn);
            AddRelationship(ahu1TwinId, level1TwinId, Relationships.LocatedIn);

            var pointId = AddTwin(CreateTwinWithSiteCode(siteCode, "Setpoint", "Point1", siteId));
            AddRelationship(pointId, ahu1TwinId, Relationships.IsCapabilityOf);

            var documentId = AddTwin(CreateTwinWithSiteCode(siteCode, "Warranty", "Warranty1", siteId));
            AddRelationship(documentId, ahu1TwinId, Relationships.IsDocumentOf);

            var ahu2TwinId = AddTwin(CreateTwinWithSiteCode(siteCode, "AirHandlingUnit", "AHU2", siteId));
            AddRelationship(ahu2TwinId, buildingTwinId, Relationships.LocatedIn);
            AddRelationship(ahu2TwinId, level2TwinId, Relationships.LocatedIn);

            return siteTwinId;
        }

        internal static string MakeId(Guid siteId, string name) => 
                    $"{name}_{siteId}";

        private static string getTestDataImportDir(string name) => 
                    Path.GetFullPath( Path.Combine( 
                        Directory.GetCurrentDirectory(), "../../../..", "TestData", name));
        

        // Import all the JSON files (each with a single model) under the Ontology import root recursively
        private void AddSharedModels()
        {
            AddSharedModels("REC");
            AddSharedModels("Willow");
        }

        private void AddSharedModels(string subDir)
        {
            var testDataRoot = getTestDataImportDir(subDir);
            var importFiles = Directory.EnumerateFiles(testDataRoot, "*.json", SearchOption.AllDirectories);

            foreach (var modelFile in importFiles)
            {
                Debug.WriteLine(modelFile);
                var model = File.ReadAllText(modelFile);
                AdtApiService.CreateModel(AdtSettings, model);
            }
        }

        public  static string ReadTestJson(string directory, string file)
        {
            var testDataRoot = getTestDataImportDir(directory);
            var path = Path.Combine(testDataRoot, file);
            return File.ReadAllText(path);
        }

        // Import array of site-specific models from file, substituting the {siteCode} template variable
        private void AddModels(string siteCode, string file = null)
        {
            var modelText = ReadTestJson("SiteSpecific", file ?? siteCode);
            modelText = Regex.Replace(modelText, "{siteCode}", siteCode);
            using var doc = JsonDocument.Parse(modelText);

            foreach (var model in doc.RootElement.EnumerateArray())
            {
                AdtApiService.CreateModel(AdtSettings, model.GetRawText());
            }
        }

        public Relationship AddRelationship(string source, string target, string relationshipName)
        {
            var relationship = new Relationship
            {
                Id = Guid.NewGuid().ToString(),
                Name = relationshipName,
                SourceId = source,
                TargetId = target
            };
            AdtApiService.AddRelationship(AdtSettings, source, relationship.Id, relationship.MapToDto());
            return relationship;
        }

        public string AddTwin(Twin twin)
        {
            return AdtApiService.AddOrUpdateTwin(AdtSettings, twin.Id, twin.MapToDto()).Result.Id;
        }

        public static Twin CreateTwinWithSiteCode(string siteCode, string modelId, string name, Guid siteId, Guid? uniqueId = null)
        {
            if (siteCode != null)
            {
                modelId = siteCode + ":" + modelId;
                name = name + "-" + siteCode;
            }

            return CreateTwin(modelId, name, siteId, uniqueId);
        }

        public static Twin CreateTwin(string modelId, string name, Guid siteId, Guid? uniqueId = null)
        {
            modelId = "dtmi:com:willowinc:" + modelId + ";1";
            return new Twin
            {
                Metadata = new TwinMetadata { ModelId = modelId },
                Id = MakeId(siteId, name),
                CustomProperties = new Dictionary<string, object>
                {
                    [Properties.Name] = name,
                    [Properties.SiteId] = siteId.ToString(),
                    [Properties.UniqueId] = uniqueId.GetValueOrDefault(Guid.NewGuid()).ToString()
                }
            };
        }

        public static Twin CreatePointTwin(string modelId, string name, Guid siteId, Guid? trendId)
        {
            modelId = "dtmi:com:willowinc:" + modelId + ";1";
            return new Twin
            {
                Metadata = new TwinMetadata { ModelId = modelId },
                Id = MakeId(siteId, name),
                CustomProperties = new Dictionary<string, object>
                {
                    [Properties.Name] = name,
                    [Properties.SiteId] = siteId.ToString(),
                    [Properties.TrendID] = trendId.GetValueOrDefault(Guid.NewGuid()).ToString()
                }
            };
        }
    }

    public static class HelperExtensions
    {
        public static TR Pipe<T, TR>(this T target, Func<T, TR> func)
        {
            return func(target);
        }
    }

}
