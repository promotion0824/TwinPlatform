// -----------------------------------------------------------------------
// <copyright file="TwinMergeHelperTests.cs" Company="Willow">
// Copyright (c) Willow, Inc.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Willow.TopologyIngestion.Test
{
    using System.Collections.Generic;
    using System.Text.Json;
    using Azure.DigitalTwins.Core;
    using Microsoft.AspNetCore.JsonPatch;
    using Willow.TopologyIngestion.AzureDigitalTwins;
    using Xunit;
    using Xunit.Abstractions;

    public class TwinMergeHelperTests
    {
#pragma warning disable IDE0052 // Remove unread private members
        private readonly ITestOutputHelper output;
#pragma warning restore IDE0052 // Remove unread private members

        public TwinMergeHelperTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void TryCreatePatchDocument_EmptyTwins_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Empty(jsonPatchDocument.Operations);
        }

        [Fact]
        public void TryCreatePatchDocument_EmptyRelationships_ReturnFalse()
        {
            var existingRelationship = new BasicRelationship();
            var newRelationship = new BasicRelationship();

            var result = TwinMergeHelper.TryCreatePatchDocument(existingRelationship, newRelationship, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Empty(jsonPatchDocument.Operations);
        }

        [Fact]
        public void TryCreatePatchDocument_DifferentModels_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            existingTwin.Metadata.ModelId = "dtmi:example:foo;1";
            existingTwin.Contents.Add("oldKey", "OldValue");
            var newTwin = new BasicDigitalTwin();
            newTwin.Metadata.ModelId = "dtmi:example:bar;1";

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);

            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/$metadata/$model", jsonPatchDocument.Operations[0].path);
            Assert.Equal("dtmi:example:bar;1", jsonPatchDocument.Operations[0].value);
        }

        [Fact]
        public void TryCreatePatchDocument_DifferentRelationshipsProperties_ReturnTrue()
        {
            var existingRelationship = new BasicRelationship();
            existingRelationship.Properties.Add("TestProperty", "This is the original value");

            var newRelationship = new BasicRelationship();
            newRelationship.Properties.Add("TestProperty", "This is the new value");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingRelationship, newRelationship, out var jsonPatchDocument);

            Assert.True(result);

            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/TestProperty", jsonPatchDocument.Operations[0].path);
            Assert.Equal("This is the new value", jsonPatchDocument.Operations[0].value);
        }

        [Fact]
        public void TryCreatePatchDocument_DifferentRelationshipsDifferentProperties_ReturnTrue()
        {
            var existingRelationship = new BasicRelationship();
            existingRelationship.Properties.Add("TestProperty1", "This is the original value");

            var newRelationship = new BasicRelationship();
            newRelationship.Properties.Add("TestProperty2", "This is the new value");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingRelationship, newRelationship, out var jsonPatchDocument);

            Assert.True(result);

            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("add", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/TestProperty2", jsonPatchDocument.Operations[0].path);
            Assert.Equal("This is the new value", jsonPatchDocument.Operations[0].value);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringTwinFieldMoreDataThanNew_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("oldKey", "OldValue");
            var newTwin = new BasicDigitalTwin();

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Empty(jsonPatchDocument.Operations);
        }

        [Fact]
        public void TryCreatePatchDocument_NewStringPropertyonNewTwin_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("add", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/newKey", jsonPatchDocument.Operations[0].path);
            Assert.Equal("NewValue", jsonPatchDocument.Operations[0].value.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringPropertyonTwinTheSame_ReturnFalse()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", "NewValue");
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.False(result);
            Assert.Empty(jsonPatchDocument.Operations);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingStringPropertyonTwinDifferent_ReturnTrue()
        {
            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", "oldValue");
            newTwin.Contents.Add("newKey", "NewValue");

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/newKey", jsonPatchDocument.Operations[0].path);
            Assert.Equal("NewValue", jsonPatchDocument.Operations[0].value);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectSingleStringPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
            };

            var existingObj = new
            {
                Vehicle = "Truck",
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/newKey/Vehicle", jsonPatchDocument.Operations[0].path);
            Assert.Equal("Car", jsonPatchDocument.Operations[0].value);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectMultipleStringPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
                Wheels = "4",
            };

            var existingObj = new
            {
                Vehicle = "Truck",
                Wheels = "6",
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal(2, jsonPatchDocument.Operations.Count);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/newKey/Vehicle", jsonPatchDocument.Operations[0].path);
            Assert.Equal("Car", jsonPatchDocument.Operations[0].value);
            Assert.Equal("replace", jsonPatchDocument.Operations[1].op);
            Assert.Equal("/newKey/Wheels", jsonPatchDocument.Operations[1].path);
            Assert.Equal("4", jsonPatchDocument.Operations[1].value);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectIntPropertyonTwinDifferent_ReturnTrue()
        {
            var newObj = new
            {
                Vehicle = "Car",
                Wheels = 4,
            };

            var existingObj = new
            {
                Vehicle = "Truck",
                Wheels = 6,
            };

            var newJson = JsonSerializer.SerializeToElement(newObj);
            var existingJson = JsonSerializer.SerializeToElement(existingObj);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("newKey", existingJson);
            newTwin.Contents.Add("newKey", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal(2, jsonPatchDocument.Operations.Count);
            Assert.Equal("replace", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/newKey/Vehicle", jsonPatchDocument.Operations[0].path);
            Assert.Equal("Car", jsonPatchDocument.Operations[0].value);
            Assert.Equal("replace", jsonPatchDocument.Operations[1].op);
            Assert.Equal("/newKey/Wheels", jsonPatchDocument.Operations[1].path);
            decimal expectedResult = 4;
            Assert.Equal(expectedResult, jsonPatchDocument.Operations[1].value);
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectArrayPropertyonTwinDifferent_ReturnTrue()
        {
            var existingIds = new[]
                {
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORGE3s6VNYCtACS519dxeFnVK", value = "urn:willowinc:twin:id:WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "ExternalIdentity", scope ="CONNECTOR", scopeId = "CON27ubR1MXxL8U2duLau65MH", value = "WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "SpaceCode", scope = "FLOOR", scopeId = "FLRWhS45cZvq9YvmBjyZesffC", value = "187B" },
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORG7bBBcM5YVJ7HiGYUULXQid", value = "urn:willowinc:twin:externalId:mappingKey:msrc%3A%2F%2FCONQe7DtU4TVNLid94BF89mzR%40willow-source%2Ftwin%2F100-LRF" },
                };

            var newIds = new[]
                {
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORGE3s6VNYCtACS519dxeFnVK", value = "urn:willowinc:twin:id:WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "ExternalIdentity", scope ="CONNECTOR", scopeId = "CON27ubR1MXxL8U2duLau65MH", value = "WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "SpaceCode", scope = "FLOOR", scopeId = "FLRWhS45cZvq9YvmBjyZesffD", value = "187A" },
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORG7bBBcM5YVJ7HiGYUULXQid", value = "urn:willowinc:twin:externalId:mappingKey:msrc%3A%2F%2FCONQe7DtU4TVNLid94BF89mzR%40willow-source%2Ftwin%2F100-LRF" },
                };

            var newJson = JsonSerializer.SerializeToElement(newIds);
            var existingJson = JsonSerializer.SerializeToElement(existingIds);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("mappedIds", existingJson);
            newTwin.Contents.Add("mappedIds", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal(2, jsonPatchDocument.Operations.Count);
            Assert.Equal("remove", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/mappedIds", jsonPatchDocument.Operations[0].path);
            Assert.Equal("add", jsonPatchDocument.Operations[1].op);
            Assert.Equal("/mappedIds", jsonPatchDocument.Operations[1].path);
            Assert.Equal(JsonSerializer.Serialize(newIds), jsonPatchDocument.Operations[1].value.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_ExistingObjectArrayPropertyonTwinNull_ReturnTrue()
        {
            var newIds = new[]
                {
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORGE3s6VNYCtACS519dxeFnVK", value = "urn:willowinc:twin:id:WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "ExternalIdentity", scope ="CONNECTOR", scopeId = "CON27ubR1MXxL8U2duLau65MH", value = "WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "SpaceCode", scope = "FLOOR", scopeId = "FLRWhS45cZvq9YvmBjyZesffD", value = "187A" },
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORG7bBBcM5YVJ7HiGYUULXQid", value = "urn:willowinc:twin:externalId:mappingKey:msrc%3A%2F%2FCONQe7DtU4TVNLid94BF89mzR%40willow-source%2Ftwin%2F100-LRF" },
                };

            var newJson = JsonSerializer.SerializeToElement(newIds);
            object? objectJson = null;
            var existingJson = JsonSerializer.SerializeToElement(objectJson);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("mappedIds", existingJson);
            newTwin.Contents.Add("mappedIds", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Equal(2, jsonPatchDocument.Operations.Count);
            Assert.Equal("remove", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/mappedIds", jsonPatchDocument.Operations[0].path);
            Assert.Equal("add", jsonPatchDocument.Operations[1].op);
            Assert.Equal("/mappedIds", jsonPatchDocument.Operations[1].path);
            Assert.Equal(JsonSerializer.Serialize(newIds), jsonPatchDocument.Operations[1].value.ToString());
        }

        [Fact]
        public void TryCreatePatchDocument_NewObjectNull_ReturnTrue()
        {
            var existingIds = new[]
                {
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORGE3s6VNYCtACS519dxeFnVK", value = "urn:willowinc:twin:id:WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "ExternalIdentity", scope ="CONNECTOR", scopeId = "CON27ubR1MXxL8U2duLau65MH", value = "WAL-WMC-A1E-L01-RM-01-187A" },
                    new { exactType = "SpaceCode", scope = "FLOOR", scopeId = "FLRWhS45cZvq9YvmBjyZesffD", value = "187A" },
                    new { exactType = "ExternalIdentity", scope = "ORG", scopeId = "ORG7bBBcM5YVJ7HiGYUULXQid", value = "urn:willowinc:twin:externalId:mappingKey:msrc%3A%2F%2FCONQe7DtU4TVNLid94BF89mzR%40willow-source%2Ftwin%2F100-LRF" },
                };

            var existingJson = JsonSerializer.SerializeToElement(existingIds);
            object? objectJson = null;
            var newJson = JsonSerializer.SerializeToElement(objectJson);

            var existingTwin = new BasicDigitalTwin();
            var newTwin = new BasicDigitalTwin();
            existingTwin.Contents.Add("mappedIds", existingJson);
            newTwin.Contents.Add("mappedIds", newJson);

            var result = TwinMergeHelper.TryCreatePatchDocument(existingTwin, newTwin, out var jsonPatchDocument);

            Assert.True(result);
            Assert.Single(jsonPatchDocument.Operations);
            Assert.Equal("remove", jsonPatchDocument.Operations[0].op);
            Assert.Equal("/mappedIds", jsonPatchDocument.Operations[0].path);
        }
    }
}
