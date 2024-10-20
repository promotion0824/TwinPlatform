Feature: Body Form Transform

Scenario: Valid transform values add key/value pair to form data
	Given the transform values contain "BodyFormParameter" with value "newKey" and "Append" with value "newValue"
	And the HTTP context has form content type with existing form data
	When the transform is built
	Then the form data should contain "newKey" with value "newValue"

Scenario: Missing BodyFormParameter returns false
	Given the transform values contain "Append" with value "newValue"
	When the transform is validated
	Then the result should be false

Scenario: Missing Append returns false
	Given the transform values contain "BodyFormParameter" with value "newKey"
	When the transform is validated
	Then the result should be false

Scenario: Valid transform values return true
	Given the transform values contain "BodyFormParameter" with value "newKey" and "Append" with value "newValue"
	When the transform is validated
	Then the result should be true
