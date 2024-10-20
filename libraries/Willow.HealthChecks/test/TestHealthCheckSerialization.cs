using FluentAssertions;

namespace Willow.HealthChecks.Test;

[TestClass]
public class TestHealthCheckSerialization
{
    private static string oldFormat = """
        {"status":"Healthy","totalDuration":"00:00:00.0038536",
        "entries":{
            "MarketplaceDbContext":{"data":{},"duration":"00:00:00.0029202","status":"Healthy","tags":[]},
            "Assembly Version":{"data":{},"description":"1.65.23.0","duration":"00:00:00.0000256","status":"Healthy","tags":[]}
            }}
        """;

    private static string sampleNewFormat = """
    {
  "Key": "Rules Web",
  "Status": 2,
  "Description": "Web health",
  "Version": "0.13.402.0",
  "Entries": {
    "Rules Engine Processor": {
      "Key": "Rules Engine Processor",
      "Status": 2,
      "Description": "Processor Health",
      "Version": "0.13.402.0",
      "Entries": {
        "Processor runtime": {
          "Key": "Processor runtime",
          "Status": 2,
          "Description": "Healthy"
        },
        "Public API": {
          "Key": "Public API",
          "Status": 2,
          "Description": "Healthy"
        },
        "Command and Control": {
          "Key": "Command and Control",
          "Status": 2,
          "Description": "Healthy"
        },
        "Service Bus": {
          "Key": "Service Bus",
          "Status": 2,
          "Description": "Healthy"
        },
        "Key Vault": {
          "Key": "Key Vault",
          "Status": 2,
          "Description": "Healthy"
        },
        "Search": {
          "Key": "Search",
          "Status": 2,
          "Description": "Healthy"
        },
        "ADX": {
          "Key": "ADX",
          "Status": 2,
          "Description": "Healthy"
        },
        "ADT": {
          "Key": "ADT",
          "Status": 2,
          "Description": "Healthy t=2843 r=4519"
        },
        "CalculatedPoints": {
          "Key": "CalculatedPoints",
          "Status": 2,
          "Description": "No calculated points processed yet"
        },
        "Git Sync": {
          "Key": "Git Sync",
          "Status": 2,
          "Description": "Up to date with remote fork"
        },
        "ADTApi": {
          "Key": "ADTApi",
          "Status": 2,
          "Description": "Healthy"
        }
      }
    },
    "Public API": {
      "Key": "Public API",
      "Status": 2,
      "Description": "Starting"
    },
    "Command API": {
      "Key": "Command API",
      "Status": 2,
      "Description": "Starting"
    },
    "Service Bus": {
      "Key": "Service Bus",
      "Status": 2,
      "Description": "Healthy"
    },
    "Key Vault": {
      "Key": "Key Vault",
      "Status": 2,
      "Description": "Healthy"
    },
    "Search": {
      "Key": "Search",
      "Status": 2,
      "Description": "Starting"
    },
    "ADX": {
      "Key": "ADX",
      "Status": 2,
      "Description": "Starting"
    },
    "ADT": {
      "Key": "ADT",
      "Status": 2,
      "Description": "Starting"
    },
    "Authorization Service": {
      "Key": "Authorization Service",
      "Status": 2,
      "Description": "Starting"
    },
    "ADTApi": {
      "Key": "ADTApi",
      "Status": 2,
      "Description": "Starting"
    }
  }
}
""";

    [TestMethod]
    public void CanDeserializeOldFormat()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        JsonSerializer jsonConverter = JsonSerializer.Create(jsonSerializerSettings);

        using JsonTextReader reader = new JsonTextReader(new StringReader(oldFormat));
        var subhealth = jsonConverter.Deserialize<HealthCheckOldStyle>(reader);

        subhealth.Should().NotBeNull();
        subhealth!.Status.Should().Be(HealthStatus.Healthy);
        subhealth.Version.Should().Be("1.65.23.0");
        subhealth.Entries.Should().HaveCount(2);
    }

    [TestMethod]
    public void CanDeserializeEitherFormat()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        JsonSerializer jsonConverter = JsonSerializer.Create(jsonSerializerSettings);

        string sample = """{"status":"Healthy","totalDuration":"00:00:00.0038536","entries":{"MarketplaceDbContext":{"data":{},"duration":"00:00:00.0029202","status":"Healthy","tags":[]},"Assembly Version":{"data":{},"description":"1.65.23.0","duration":"00:00:00.0000256","status":"Healthy","tags":[]}}}""";

        var result = HealthCheckFederated.Parse(jsonConverter, sample);

        result.Should().NotBeNull();
        result!.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().HaveCount(1);
    }

    [TestMethod]
    public void CanDeserializeEitherFormatNew()
    {
        JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
        JsonSerializer jsonConverter = JsonSerializer.Create(jsonSerializerSettings);

        var result = HealthCheckFederated.Parse(jsonConverter, sampleNewFormat);

        result.Should().NotBeNull();
        result!.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().HaveCount(11);
        result.Data["Version"].Should().Be("0.13.402.0");
    }
}
