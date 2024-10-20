Feature: Twin Service
	Service for adding and retrieving twins from a memory cache

Background:
	Given I have twins in the cache
	  | Id   | ModelId         | ConnectorId     | ExternalId | Unit     | Enabled | TrendInterval |
	  | TW-1 | TestSensorModel | TestConnector-1 | PNT123     | TestUnit | true    | 900           |
	  | TW-3 | TestSensorModel | TestConnector-1 | PNT456     | TestUnit | true    | 900           |
	  | TW-4 | TestSensorModel | TestConnector-2 | PNTx7cA    | TestUnit | false   | 900           |
	  | TW-5 | TestSensorModel | TestConnector-3 |            | TestUnit | false   | 900           |

@Unit @AB#132143
Scenario: Get a twin from the cache
	When I call GetTwin with Id '<ExternalId>'
	Then I should get the following twin:
	  | Id   | ModelId         | ConnectorId   | ExternalId   | Unit     | Enabled   | TrendInterval |
	  | <Id> | TestSensorModel | <ConnectorId> | <ExternalId> | TestUnit | <Enabled> | 900           |

	Examples:
	  | Id   | ExternalId | ConnectorId     | Enabled |
	  | TW-1 | PNT123     | TestConnector-1 | true    |
	  | TW-3 | PNT456     | TestConnector-1 | true    |
	  | TW-4 | PNTx7cA    | TestConnector-2 | false   |

@Unit @AB#132143
Scenario: Get a twin that does not exist
	When I call GetTwin with Id '<Id>'
	Then I should get no twin

	Examples:
		| Id   |
		| TW-2 |
		| TW-6 |
 		| TW-7 |
