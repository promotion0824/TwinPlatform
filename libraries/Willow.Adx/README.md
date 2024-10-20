# Willow.Adx

A simple wrapper around the Kusto library.

## Getting started

Add the following to your host builder, replacing the "Adx" section name if required:

```csharp
services.AddWillowAdxService(options => Configuration.Bind("Adx", options));
```

## Usage

Inject `IAdxService` into your class and use the `Query` method to run queries.

```csharp
public class MyClass(IAdxService adxService)
{
	public async Task RunQuery()
	{
		var result = await _adxService.QueryAsync<MyDto>(
			@"declare query_parameters(timespanago : timespan);
			ActiveTwins
			| where SourceTimestamp > ago(timespanago)",
			new Dictionary<string, string>
			{
				{ "timespanago", "1d" }
			});
	}
}
```
