# Telemetry Streaming

Receives telemetry from an Event Hub and forwards them to an MQTT broker.

## Getting Started

### Prerequisites

An Event Grid Namespaces instance with MQTT enabled and an intermediate certificate uploaded.
A Key Vault with a willow.pfx file, signed by the intermediate certificate.
An Event Hub the receives telemetry in Willow format.
An Azure Storage Table for storing subscriptions.

### Quickstart

Set ```"DevMode": true``` in configuration. This will use an Event Hub and storage account emulator.

### App Settings

Example configuration using the CI environment.

```json
{
  "EventHub": {
    "Source": {
      "Name": "evh-ingestion-to-adx",
      "FullyQualifiedNamespace": "evhns-dev-eus-01-wil-in1-43907969.servicebus.windows.net",
      "ConsumerGroup": "cg-ingestion-to-adx-telemetry-streaming",
      "StorageAccountUri": "https://stodeveus01wili3c46beea.blob.core.windows.net",
      "StorageContainerName": "eventhub"
    }
  },
  "Mqtt": {
    "Server": "evgns-dev-eus-01-wil-in1-ebea5a2a.eastus-1.ts.eventgrid.azure.net",
    "Port": 8883,
    "ClientId": "willow",
    "AuthenticationMethod": "ClientCertificate",
    "CertificateAuthentication": {
      "KeyVault": "https://kvdeveus01wilifab32151.vault.azure.net",
      "CertificateName": "willow"
    }
  },
  "Subscriptions": {
    "StorageAccountUri": "https://stodeveus01wili3c46beea.table.core.windows.net",
    "StorageTableName": "TelemetryStreamingSubscriptions"
  }
}
```





