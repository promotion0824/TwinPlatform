Feature: TimeSeries Monitoring
	Implements monitoring of a buffer of timed values and their tracking status.

@Unit @AB#90766
Scenario: Validation of out-of-range temperature sensor readings
	Given I have a temperature sensor with Unit <Unit>
	When the incoming value is <Value>
	Then the out of range validation should be <Validation>

Examples:
  | Unit       | Value | Validation |
  | Celsius    | 100   | false      |
  | Celsius    | -50   | false      |
  | Celsius    | 10000 | true       |
  | Celsius    | -100  | true       |
  | Fahrenheit | 300   | false      |
  | Fahrenheit | -100  | false      |
  | Fahrenheit | 1000  | true       |
  | Fahrenheit | -200  | true       |

Scenario: Incoming value is invalid
	Given I have a modeled capability in ADT
	When invalid incoming value is "SomeRandomString"
	Then the validation result should be false

Scenario: Invalid to Valid transition
	Given I have a modeled capability in ADT
	When invalid incoming value is "SomeRandomString"
	Then the validation result should be false
	When a new valid value is received
	Then the validation result should be true

Scenario: Incoming value does not have twin
	Given I have a temperature sensor with Unit Celsius
	When there is no twin modelled
	Then the no twin validation should be true

Scenario: Incoming value is for a modelled twin
	Given I have a modeled capability in ADT
	When a new valid value is received
	Then the no twin validation should be false

Scenario: Capability has gone Offline
	Given I have a modeled capability in ADT
	When no new value is received for more than 3x the trendInterval
	Then the offline validation result should be true

Scenario: Capability comes back online after being offline
	Given I have a modeled capability in ADT
	When no new value is received for more than 3x the trendInterval
	Then the offline validation result should be true
	When a new valid value is received
	Then the offline validation result should be false
