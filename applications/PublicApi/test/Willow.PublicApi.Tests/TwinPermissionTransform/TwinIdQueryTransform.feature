Feature: Twin ID Query Transform

Background:
	Given I have permission to the following twin IDs
		| Twin ID | External ID |
		| Twin1   |             |
		| Twin2   |             |
	And I have a query transform with ID "twinId"

@Unit @AB-133460
Scenario: Validate Query Transform
	When I validate the query transform
	Then the result will be true

@Unit @AB-133460
Scenario: Build Query Transform
	When I build the query transform
	Then the result will be true

@Unit @AB-133460
Scenario: Execute Query Transform - Single Value
	When I execute the query transform with "<Twin ID>"
	Then the response status code will be <Status Code>

Examples:
	| Twin ID | Status Code |
	| Twin1   | 200         |
	| Twin2   | 200         |
	| Twin3   | 403         |

@Unit @AB-133460
Scenario: Execute Query Transform - Multiple Values
	When I execute the query transform with multiple values "<Twin IDs>"
	Then the query string for "twinId" will have values "<Expected Twin IDs>"
	And the response status code will be <Status Code>

Examples:
	| Twin IDs    | Status Code | Expected Twin IDs |
	| Twin1,Twin2 | 200         | Twin1,Twin2       |
	| Twin2,Twin3 | 200         | Twin2             |

@Unit @AB-133460
Scenario: Execute Query Transform - Multiple Values - All invalid
	When I execute the query transform with multiple values "<Twin IDs>"
	Then the response status code will be <Status Code>

Examples:
	| Twin IDs    | Status Code |
	| Twin3,Twin4 | 403         |
