Feature: Twin ID and External ID Body Transform

Background:
	Given I have permission to the following twin IDs
		| Twin ID | External ID |
		| Twin1   | Ext1        |
		| Twin2   | Ext2        |
	And I have a body transform with ID "twinId" and external ID "externalId"

@Unit @AB-133460
Scenario: Validate Body Transform
	When I validate the body transform
	Then the result will be true

@Unit @AB-133460
Scenario: Validate Body Transform - Fail
	Given I have a body transform with ID "<Twin ID Name>" and external ID "<External ID Name>"
	When I validate the body transform
	Then the result will be false

Examples:
	| Twin ID Name | External ID Name |
	|              |                  |
	| twinId       |                  |
	|              | externalId       |

@Unit @AB-133460
Scenario: Build Body Transform
	When I build the body transform
	Then the result will be true

@Unit @AB-133460
Scenario: Build Body Transform - Fail
	Given I have a body transform with ID "<Twin ID Name>" and external ID "<External ID Name>"
	When I build the body transform
	Then the result will be false

Examples:
	| Twin ID Name | External ID Name |
	|              |                  |
	| twinId       |                  |
	|              | externalId       |

@Unit @AB-133460
Scenario: Execute Body Transform - Single, Named
	When I execute the body transform with a single value "<Twin ID>" and external ID "<External ID>"
	Then the JSON property for "twinId" will have the value "<Expected Twin ID>"
	And the JSON property for "externalId" will have the value "<Expected External ID>"
	And the response status code will be <Status Code>

Examples: b
	| Twin ID | External ID | Status Code | Expected Twin ID | Expected External ID |
	| Twin1   | Ext1        | 200         | Twin1            | Ext1                 |
	|         | Ext1        | 200         |                  | Ext1                 |

@Unit @AB-133460
Scenario: Execute Body Transform - Array of objects, Single, Named
	When I execute the body transform with an array of objects with the single value "<Twin IDs>" and external ID "<External IDs>"
	Then the array of objects will have JSON properties for "twinId" and will have the values "<Expected Twin IDs>"
	Then the array of objects will have JSON properties for "externalId" and will have the values "<Expected External IDs>"
	And the response status code will be <Status Code>
Examples:
	| Twin IDs    | External IDs | Status Code | Expected Twin IDs | Expected External IDs |
	| Twin1       | Ext1         | 200         | Twin1             | Ext1                  |
	| Twin1       | Ext3         | 200         | Twin1             | Ext3                  |
	| Twin1,Twin2 | Ext1,Ext2    | 200         | Twin1,Twin2       | Ext1,Ext2             |
	| Twin2,Twin3 | Ext2,Ext3    | 200         | Twin2             | Ext2                  |
	|             | Ext1,Ext2    | 200         | ,                 | Ext1,Ext2             |
	|             | Ext2,Ext3    | 200         |                   | Ext2                  |

@Unit @AB-133460
Scenario: Execute Body Transform - Single Fail
	When I execute the body transform with a single value "<Twin ID>" and external ID "<External ID>"
	Then the response status code will be <Status Code>

Examples:
	| Twin IDs | External IDs | Status Code |
	# Fails if Twin ID is invalid, regardless of external ID
	| Twin3    | Ext2         | 403         |
	| Twin3    | Ext3         | 403         |
	|          | Ext3         | 403         |


@Unit @AB-133460
Scenario: Execute Body Transform - Multiple Fail
	When I execute the body transform with an array of objects with the single value "<Twin IDs>" and external ID "<External IDs>"
	Then the response status code will be <Status Code>

Examples:
	| Twin IDs    | External IDs | Status Code |
	# Fails if Twin ID is invalid, regardless of external ID
	| Twin3       | Ext2         | 403         |
	| Twin3       | Ext3         | 403         |
	| Twin3,Twin4 | Ext3,Ext4    | 403         |
	|             | Ext3         | 403         |
	|             | Ext3,Ext4    | 403         |
