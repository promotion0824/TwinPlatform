Feature: Null Values

@Unit
Scenario: Null String
	Given a string "$null"
	Then the value is null

Scenario: Null Integer
	Given an int $null
	Then the value is null

Scenario: Null Double
	Given a double $null
	Then the value is null

Scenario: Null Decimal
	Given a decimal $null
	Then the value is null

Scenario: Null Boolean
	Given a boolean $null
	Then the value is null
