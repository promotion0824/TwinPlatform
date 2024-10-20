Feature: Telemetry Processor

@Unit @AB#90735
Scenario: Connector ID is null
	Given I have telemetry
	And the connector ID is null
	When I call ProcessAsync
	Then the telemetry is not processed

@Unit @AB#90735
Scenario: External ID is null
	Given I have telemetry
	And the external ID is null
	When I call ProcessAsync
	Then the telemetry is not processed

@Unit @AB#90735
Scenario: Scalar value is not a double
	Given I have telemetry
	And the scalar value is a string
	When I call ProcessAsync
	Then the telemetry is not processed

@Unit @AB#90735
Scenario: No matching subscriptons
	Given I have telemetry
	And I do not have a matching subscription
	When I call ProcessAsync
	Then the telemetry is not processed

@Unit @AB#90735
Scenario: Matching subscriptons
	Given I have telemetry
	And I have a matching subscription
	When I call ProcessAsync
	Then the telemetry is processed
