# Willow.SpecFlow

Increases the usability of SpecFlow.

## Getting started

Add or update a file named `specflow.json` in the root of your project. The file should be marked as `Content` and `Copy if newer`.
The file should have the following content:

```json
{
  "stepAssemblies": [
    {
      "assembly": "Willow.SpecFlow"
    }
  ]
}
```

## DateTime Expressions

_Note that the intention here is to unlock the full power of Willow Expressions. As these do not support date arithmetic at time of writing, this library has created a limited implementation to support the most immediate need._

To provide non-specific date and time values in your feature files, you can use the following expressions:

- `NOW` - The current UTC date and time
- `TODAY` - The current UTC date

You can perform simple arithmatic, adding or subtracting days, hours, minutes or seconds:

- `TODAY + 1d` - Add a day to the current date
- `NOW - 1h` - Subtract an hour from the current date and time

Supported units are:

- `d`, `day` or `days`
- `h`, `hour` or `hours`
- `m`, `min` or `mins`
- `s`, `sec`, `secs`

Null values are also supported, but leaving the parameter blank.

### Examples

```gherkin
Scenario: Demonstration
Given the object's updated date is "TODAY - 1day"
When I update the object
Then the object's updated date is "TODAY"
```

In the step definition, you can just use a `DateTime`.

```csharp
[Given("the object's updated date is \"(.*)\""]
public void GivenTheObjectsUpdatedDateIs(DateTime updatedDate)
{
	// Set the object's updated date
}

[Then("the object's updated date is \"(.*)\""]
public void ThenTheObjectsUpdatedDateIs(DateTime updatedDate)
{
	// Check the object's updated date
}
```

## Willow.Expressions TimeProvider

This code uses the Willow.Expressions library to provide the date and time values for `NOW` and `TODAY`.

To provide your own date and time values, you can use the `ManualTimeProvider` class.
This class allows you to set the current date and time to a specific value.

```csharp
Willow.Units.TimeProvider.Current = new ManualTimeProvider(DateTime.Now);
```

## Passing `null` Values
By default passing no value in a table or as a parameter will result in an empty string value. If you want to pass null, you can use the string `$null`.

```gherkin
Scenario: Throw exception if null or empty
    Given the object's name is "<Name>"
    When I update the object
    Then an exception is thrown
Examples:
    | Name  |
    | $null |
    |       |
```
