Feature: Test Notification Extensions

Scenario: Notification is formatted correctly for Teams
	Given I have a Notification
	When I call ToTeamsMessageString
	Then the message is formatted correctly for Teams

Scenario: Notification is formatted correctly for Email
	Given I have a Notification
	When I call ToEmailBodyString
	Then the message is formatted correctly for Email
