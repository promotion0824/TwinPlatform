Feature: Twin ID Route Transform

@Unit @AB-133460
Scenario: Validate Route Transform
	Given I have a route transform with ID "twinId"
	When I validate the route transform
	Then the result will be true

Scenario: Build Route Transform
	Given I have a route transform with ID "twinId"
	When I build the route transform
	Then the result will be true

@Unit @AB-133460
Scenario: Execute Route Transform
	Given I have permission to the following twin IDs
	| Twin ID | External ID |
	| Twin1   |             |
	| Twin2   |             |
	And I have a route transform with ID "twinId"
	When I execute the route transform with "<Twin ID>"
	Then the response status code will be <Status Code>
	Examples:
	| Twin ID | Status Code |
	| Twin1   | 200         |
	| Twin2   | 200         |
	| Twin3   | 403         |
