# Cognian Adapter

## Description

The Cognian Adapter is designed to substitute the [Azure Stream Analytics Job](https://github.com/WillowInc/Connectors-LiveData/tree/677d85ab127f1fd368c424206f6b3925e809de29/StreamingAnalytics/Investa/FourTwentyGeorgeStreet/Cognian.StreamAnalytics).
It retrieves data from the IoT Hub, standardizes the messages into a unified format, and then sends them on to Event Hub.

For more details about the Cognian Syncromesh Solution, please see [Cognian Synchromesh Solution](https://willow.atlassian.net/wiki/spaces/INTEG/pages/1723138602/Cognian+Syncromesh+Solution).

## Configuration

These values can be read either from appsettings.json

``` json

  "IoTHubService": {
    "ConnectionString": ""
  },
  "CognianAdapter": {
    "ConnectorId": ""
  },
  "EventHub": {
    "Source": {
      "Name": "",
      "FullyQualifiedNamespace": "",
      "ConsumerGroup": "",
      "StorageAccountUri": "",
      "StorageContainerName": "",
      "ConnectionString": ""
    },
    "Destination": {
      "Name": "",
      "FullyQualifiedNamespace": ""
    }
  }
```
