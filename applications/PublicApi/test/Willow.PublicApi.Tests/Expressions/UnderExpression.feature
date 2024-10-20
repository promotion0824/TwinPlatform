Feature: Under Expression

@Unit @AB-133460
Scenario: Under Expression
	Given I have an expression "UNDER([my-twin-id])"
	When I visit it with the TwinQueryVisitor
	Then the a QueryResult is returned
	And success is true
	And the query is null
	And the Request is not null
	And the Request.LocationId property is "my-twin-id"
