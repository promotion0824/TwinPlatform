Feature: Resource Checker
  As a user
  I want to check permissions and filter twin IDs
  So that I can ensure access control for digital twins

Background:
	Given a client ID "client-123"
	And an expression resolver with expressions for the client ID
	And a twins client with twin data
		| Twin ID      | External ID  |
		| location-123 | extloc-123   |
		| twin-123     | external-123 |
		| twin-456     | external-456 |
	And a cache service

Scenario: Check if a user has permission for a specific twin ID
	Given a twin ID "twin-123"
	When I check for twin permission
	Then the result should be true

Scenario: Check if a user has permission for a specific external ID
	Given an external ID "external-123"
	When I check for external ID permission
	Then the result should be true

Scenario: Filter twin IDs based on user permissions
	Given a list of twin IDs
		| Twin ID  |
		| twin-123 |
		| twin-456 |
		| twin-789 |
	When I filter twin IDs based on permissions
	Then the result should contain
		| Twin ID      |
		| twin-123     |
		| twin-456     |

Scenario: Filter external IDs based on user permissions
	Given a list of external IDs
		| External ID  |
		| external-123 |
		| external-456 |
		| external-789 |
	When I filter external IDs based on permissions
	Then the result should contain
		| External ID  |
		| external-123 |
		| external-456 |

Scenario: Get allowed twins for a user
	When I get allowed twins
	Then the result should contain twin IDs and external IDs
		| Twin ID      | External ID  |
		| location-123 | extloc-123   |
		| twin-123     | external-123 |
		| twin-456     | external-456 |
