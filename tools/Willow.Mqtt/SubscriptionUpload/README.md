# Subscription Upload

Uploads Telemetry Streaming subscriptions to an Azure Storage Table.

```
Description:
  Uploads Telemetry Streaming subscriptions to an Azure Storage Table.

Usage:
  SubscriptionUpload [options]

Options:
  -f, --file <file> (REQUIRED)                           The CSV file containing the subscriptions
  -s, --subscription <subscription> (REQUIRED)           The MQTT subscription ID
  -c, --conn <connecxtion string> (REQUIRED)             The Azure Storage connection string
  -?, -h, --help                                         Show help and usage information
```
