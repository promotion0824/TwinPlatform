# Willow.SpecFlow.Xunit

SpecFlow bindings that offer simple assertions.

## Getting started

Add or update a file named `specflow.json` in the root of your project. The file should be marked as `Content` and `Copy if newer`.
The file should have the following content:

```json
{
  "stepAssemblies": [
    {
      "assembly": "Willow.SpecFlow.Xunit"
    }
  ]
}
```

## Simple Assertions

Simple assertions provide equality checks for:

| Type       | Example                            |
|------------|------------------------------------|
| `string`   | The string value "..." is returned |
| `int`      | The integer value ... is returned  |
| `double`   | The double value ... is returned   |
| `decimal`  | The decimal value ... is returned  |
| `bool`     | The boolean value ... is returned  |
| `DateTime` | The date "..." is returned         |
| `null`     | The value is null                  |

The `string` and `DateTime` expressions also support single quotes `'`.

To use:

- Add `Then` statements in the correct format to your feature files for out-of-the-box assertions.
- Inject a `ScenarioContext` into your bindings and call `AddResult`.


### Example

```gherkin
Scenario: Demonstration
    Given: I have a string "Hello"
    When: I add the string " World"
	Then: The string value "Hello World" is returned
```

```csharp
[Binding]
public class MyBinding(ScenarioContext context)
{
    private string start;

    [Given("I have a string \"(.*)\"")]
    public void GivenIHaveAString(string value)
    {
	    start = value;
    }

    [When("I add the string \"(.*)\"")]
    public void WhenIAddTheString(string value)
    {
	    context.AddResult(start + value);
    }
}
```
