# Telemetry Data Quality

Processes telemetry ingested into the unified eventhub and runs various data qualtiy validations against timeseries data
and sends the results to TelemetryDataQuality eventhub.

## Getting Started

### Prerequisites

A consumer group to read telemetry from the unified eventhub.
Send access to the TelemetryDataQuality eventhub.

### App Settings

Example configuration using the CI environment.

```json
{
  "EventHub": {
    "Source": {
      "Name": "evh-ingestion-to-adx",
      "FullyQualifiedNamespace": "evhns-dev-eus-01-wil-in1-43907969.servicebus.windows.net",
      "ConsumerGroup": "cg-ingestion-to-adx-telemetry-data-quality",
      "StorageAccountUri": "https://stodeveus01wili3c46beea.blob.core.windows.net",
      "StorageContainerName": "eventhub-data-quality"
    },
    "Destination": {
      "Name": "evh-telemetry-data-quality",
      "FullyQualifiedNamespace": "evhns-dev-eus-01-wil-in1-43907969.servicebus.windows.net"
    }
  },
  "Adx": {
    "ClusterUri": "https://dec-dev-eus-01.eastus.kusto.windows.net",
    "DatabaseName": "dedb-dev-eus-01-wil-in1"
  }
}
```





