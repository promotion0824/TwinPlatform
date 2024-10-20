Feature: Transform Validation

@Unit @AB-133460
Scenario: Validate Body Transform - one param
	Given I have a transform of type "<Type>" and value "<Value>"
	When I validate the transform
	Then the result will be <Result>

	Examples:
	| Type           | Value | Result |
	| TwinIdRoute    |       | false  |
	| TwinIdRoute    | route | true   |
	| TwinIdQuery    |       | false  |
	| TwinIdQuery    | query | true   |
	| TwinIdBody     |       | true   |
	| TwinIdBody     | body  | true   |
	| ExternalIdBody |       | false  |
	| ExternalIdBody | ext   | true   |
	| InvalidKey     | inv   | false  |

@Unit @AB-133460
Scenario: Validate Body Transform - two params
	Given I have a transform of type "<Type 1>" and value "<Value 1>"
	And I have a transform of type "<Type 2>" and value "<Value 2>"
	When I validate the transform
	Then the result will be <Result>

Examples:
| Type 1      | Value 1 | Type 2         | Value 2 | Result |
| TwinIdRoute | route   | TwinIdQuery    | query   | false  |
| TwinIdRoute | route   | TwinIdBody     | body    | false  |
| TwinIdQuery | query   | TwinIdBody     | body    | false  |
| TwinIdBody  | body    | ExternalIdBody | ext     | true   |
| TwinIdRoute | route   | InvalidKey     | inv     | false  |
