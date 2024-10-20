Feature: Retrieving subscriptions from table storage

@Unit @AB#90735
Scenario: Retrieves matching subscriptions only
	Given I have a table storage with the following subscriptions:
		| PartitionKey | ConnectorId                          | ExternalId |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
		| test-sub2    | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
		| test-sub2    | 64cb468d-0229-4956-8ddf-c586be43edd0 | 123456789  |
		| test-sub     | 23165b82-5c27-4bd1-bb3d-53ee96086199 | 701625AO0  |
	When I call GetSubscriptions with connector ID '64cb468d-0229-4956-8ddf-c586be43edd0' and external ID '701625AO0'
	Then I should get the following subscriptions:
		| SubscriberId | ConnectorId                          | ExternalId |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
		| test-sub2    | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |

@Unit @AB#90735
Scenario: Retrieving subscriptions from table storage
	Given I have a table storage with the following subscriptions:
		| PartitionKey | ConnectorId                          | ExternalId |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
	When I call GetSubscriptions with connector ID '64cb468d-0229-4956-8ddf-c586be43edd0' and external ID '701625AO0'
	Then I should get the following subscriptions:
		| SubscriberId | ConnectorId                          | ExternalId |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
	And the metadata object will be null

@Unit @AB#90735
Scenario: Retrieving subscriptions from table storage with metadata
	Given I have a table storage with the following subscriptions:
		| PartitionKey | ConnectorId                          | ExternalId | TestField1 | TestField2 |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  | TestValue1 | TestValue2 |
	When I call GetSubscriptions with connector ID '64cb468d-0229-4956-8ddf-c586be43edd0' and external ID '701625AO0'
	Then I should get the following subscriptions:
		| SubscriberId | ConnectorId                          | ExternalId |
		| test-sub     | 64cb468d-0229-4956-8ddf-c586be43edd0 | 701625AO0  |
	And the metadata object will be have the following properties:
		| TestField1 | TestField2 |
		| TestValue1 | TestValue2 |
