Feature: HttpContext Extensions
Get the client ID from a JWT token or a form body

@Unit
Scenario: Valid token returns client ID
	Given the HTTP context has a valid token with client ID "test-client-id"
	When the client ID is retrieved from the token
	Then the client ID should be "test-client-id"

@Unit
Scenario: Missing authorization header returns null
	Given the HTTP context has no authorization header
	When the client ID is retrieved from the token
	Then the client ID should be null

@Unit
Scenario: Non-bearer authorization header returns null
	Given the HTTP context has an invalid authorization header
	When the client ID is retrieved from the token
	Then the client ID should be null

@Unit
Scenario: Form contains client ID
	Given the HTTP context has a form with client ID "test-client-id"
	When the client ID is retrieved from the body
	Then the client ID should be "test-client-id"

@Unit
Scenario: Form does not contain client ID
	Given the HTTP context has a form without client ID
	When the client ID is retrieved from the body
	Then the client ID should be null

@Unit
Scenario: Request is not form content type
	Given the HTTP context is not form content type
	When the client ID is retrieved from the body
	Then the client ID should be null
