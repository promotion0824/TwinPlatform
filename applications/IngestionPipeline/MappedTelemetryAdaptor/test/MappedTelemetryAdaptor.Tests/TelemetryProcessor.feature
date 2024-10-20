Feature: Telemetry Processor
Processor for Mapped Telemetry

    @Unit
    @AB#93066
    Scenario: Value is Scalar
        Given I have Mapped Telemetry
        And the Value is scalar
        When I call ProcessMappedTelemetry
        Then the properties should be null
        And the ScalarValue should be Value

    @Unit
    @AB#93066
    Scenario: Value is Json
        Given I have Mapped Telemetry
        And the Value is Json
        When I call ProcessMappedTelemetry
        Then the properties should be Json
        And the ScalarValue should be 1
