# Willow.Caching

Library to support caching implementations for various applications.

## TimeSeries Cache

Key is of the format `{Prefix}:{version}:{externalId}`, where Prefix is `TimeSeries` and
version is the data model version.

Get the key used for Redis Cache `TimeSeriesKeys.GetKey("v1","externalId")`.
The result if available from Redis can be deserialized as `TimeSeries` class.

It will be the responsibility of the producer to ensure backwards compatability of the key versions.
