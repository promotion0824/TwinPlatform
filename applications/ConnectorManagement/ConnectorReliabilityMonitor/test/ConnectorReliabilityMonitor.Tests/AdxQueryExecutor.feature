Feature: AdxQueryExecutor
    In order to verify query execution
    As a developer
    I want to ensure that queries are executed and logged correctly

Scenario: Execute queries for given Connector
    Given I have a Connector configured with "ConnectorId:TestConnectorId, Interval:300, Name:TestConnectorName"
    And a query is configured in AdxQueryExecutor
    When the query is executed
    Then the result should contain the configured ConnectorName
