Feature: Twin ID Body Transform

Background:
	Given I have permission to the following twin IDs
		| Twin ID | External ID |
		| Twin1   |             |
		| Twin2   |             |

@Unit @AB-133460
Scenario: Build Body Transform
	Given I have a body transform with ID "<Twin ID>"
	When I build the body transform
	Then the result will be true
Examples:
	| Twin ID |
	| Twin1   |
	|         |

@Unit @AB-133460
Scenario: Execute Body Transform - Multiple, Unnamed
	Given I have a body transform with ID ""
	When I execute the body transform with multiple values "<Twin IDs>"
	Then the JSON property for "" will have values "<Expected Twin IDs>"
	And the response status code will be <Status Code>

Examples:
	| Twin IDs    | Status Code | Expected Twin IDs |
	| Twin1,Twin2 | 200         | Twin1,Twin2       |
	| Twin2,Twin3 | 200         | Twin2             |

@Unit @AB-133460
Scenario: Execute Body Transform - Multiple, Named
	Given I have a body transform with ID "twinId"
	When I execute the body transform with multiple values "<Twin IDs>"
	Then the JSON property for "twinId" will have values "<Expected Twin IDs>"
	And the response status code will be <Status Code>

Examples:
	| Twin IDs    | Status Code | Expected Twin IDs |
	| Twin1,Twin2 | 200         | Twin1,Twin2       |
	| Twin2,Twin3 | 200         | Twin2             |

@Unit @AB-133460
Scenario: Execute Body Transform - Single, Named
	Given I have a body transform with ID "twinId"
	When I execute the body transform with a single value "<Twin ID>"
	Then the JSON property for "twinId" will have the value "<Expected Twin ID>"
	And the response status code will be <Status Code>

Examples:
	| Twin ID | Status Code | Expected Twin ID |
	| Twin1   | 200         | Twin1            |

@Unit @AB-133460
Scenario: Execute Body Transform - Array of objects, Single, Named
	Given I have a body transform with ID "twinId"
	When I execute the body transform with an array of objects with the single value "<Twin IDs>"
	Then the array of objects will have JSON properties for "twinId" and will have the values "<Expected Twin IDs>"
	And the response status code will be <Status Code>
Examples:
	| Twin IDs    | Status Code | Expected Twin IDs |
	| Twin1,Twin2 | 200         | Twin1,Twin2       |
	| Twin2,Twin3 | 200         | Twin2             |

@Unit @AB-133460
Scenario: Execute Body Transform - Fail
	Given I have a body transform with ID "<Property ID>"
	When I execute the body transform with multiple values "<Twin IDs>"
	Then the response status code will be <Status Code>

Examples:
	| Property ID | Twin IDs    | Status Code |
	|             | Twin3       | 403         |
	|             | Twin3,Twin4 | 403         |
	| twinId      | Twin3       | 403         |
	| twinId      | Twin3,Twin4 | 403         |
