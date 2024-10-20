# Custom IoTHub Routing Adapter

This is a custom app intended to be used for routing configured connectorId messages from one IoTHub to another for testing.

This is intended to be used only for DDK customer instance demo purposes and not for general production rollout.

### App Settings

Example configuration using the CI environment.

```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=196429E1-6F70-454E-B127-C1FDFEE6D365"
  },
  "EventHub": {
    "Source": {
      "Name": "iot-dev-eus-01-wil-in1-ab",
      "FullyQualifiedNamespace": "iothub-ns-iot-dev-eu-25284789-b50076d087.servicebus.windows.net",
      "ConsumerGroup": "$Default",
      "StorageAccountUri": "https://stodeveus01wili3c46beea.blob.core.windows.net",
      "StorageContainerName": "custom-iothub-routing-checkpoint-storage"
    },
    "Destination": {
      "Name": "evh-ingestion-to-adx",
      "FullyQualifiedNamespace": "evhns-dev-eus-01-wil-in1-43907969.servicebus.windows.net"
    }
  },
  "ConnectorIdList": ["df856cbc-e014-498d-8da0-6ce868ca7cd6", "2f28557b-e3bd-40ce-806f-5b263cecda97"]
}
```





