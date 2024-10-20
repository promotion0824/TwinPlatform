Feature: GetAdxQueries
	In order to ensure that the GetAdxQueries method works correctly
	As a developer
	I want to make sure that the method replaces placeholders correctly and returns the correct queries

Scenario: GetAdxQueries should return the correct count of queries
	When I call GetAdxQueries
	Then the result should contain 7 queries
