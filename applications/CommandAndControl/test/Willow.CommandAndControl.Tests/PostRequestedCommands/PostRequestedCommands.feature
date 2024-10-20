Feature: Requested Command conflict resolution

@Unit
Scenario Outline: Test Transformation
	Given I have a command with
		| TwinId | RuleId | Value | Type    | StartTime             | EndTime             | Status   | ConnectorId |
		| TW-12  | Rule-1 | 1.0   | atLeast | <Existing Start Time> | <Existing End Time> | <Status> | Connector   |

	When I receive a command with
		| TwinId | RuleId | Value | Type    | StartTime        | EndTime        | ConnectorId |
		| TW-12  | Rule-1 | 1.0   | atLeast | <New Start Time> | <New End Time> | Connector   |

	Then Nothing Happens

Examples:
	| Existing Start Time | New Start Time | Existing End Time | New End Time  | Status  |
    # Same start and end times
	| TODAY - 1days       | TODAY - 1days  | TODAY + 1days     | TODAY + 1days | Pending |



@Unit
Scenario Outline: Command received with same value and different timings within the bounds of the existing command results in no change
	Given I have a command with
		| TwinId | RuleId | Value | Type    | StartTime             | EndTime             | Status   | ConnectorId | ExternalId | CommandName |
		| TW-12  | Rule-1 | 1.0   | atLeast | <Existing Start Time> | <Existing End Time> | <Status> | Connector   | EXT        | Command 1   |

	When I receive a command with
		| TwinId | RuleId | Value | Type    | StartTime        | EndTime        | ConnectorId | ExternalId | CommandName |
		| TW-12  | Rule-1 | 1.0   | atLeast | <New Start Time> | <New End Time> | Connector   | EXT        | Command 1   |

	Then Nothing Happens

Examples:
	| Existing Start Time | New Start Time  | Existing End Time | New End Time | Status   |
    # Same start and end times
	| TODAY - 1day        | TODAY - 1day    | TODAY + 1day      | TODAY + 1day | Pending  |
    # Same start time, no end times
	| TODAY - 1day        | TODAY - 1day    |                   |              | Pending  |
    # New start time is later
	| TODAY - 1day        | TODAY - 12hours | TODAY + 1day      | TODAY + 1day | Pending  |
    # New start time is later, no end times
	| TODAY - 1day        | TODAY - 12hours |                   |              | Pending  |
    # Approved
    # Same start and end times
	| TODAY - 1day        | TODAY - 1day    | TODAY + 1day      | TODAY + 1day | Approved |
    # Same start time, no end times
	| TODAY - 1day        | TODAY - 1day    |                   |              | Approved |
    # New start time is later
	| TODAY - 1day        | TODAY - 12hours | TODAY + 1day      | TODAY + 1day | Approved |
    # New start time is later, no end times
	| TODAY - 1day        | TODAY - 12hours |                   |              | Approved |
    # Rejected
    # Same start and end times
	| TODAY - 1day        | TODAY - 1day    | TODAY + 1day      | TODAY + 1day | Rejected |
    # Same start time, no end times
	| TODAY - 1day        | TODAY - 1day    |                   |              | Rejected |
    # New start time is later
	| TODAY - 1day        | TODAY - 12hours | TODAY + 1day      | TODAY + 1day | Rejected |
    # New start time is later, no end times
	| TODAY - 1day        | TODAY - 12hours |                   |              | Rejected |

@Unit
Scenario Outline: Command received with same value and different timings outside the bounds of the existing command results in a new command
	Given I have a command with
		| TwinId | RuleId | Value | Type    | StartTime             | EndTime             | Status   |
		| TW-12  | Rule-1 | 1.0   | atLeast | <Existing Start Time> | <Existing End Time> | <Status> |

	When I receive a command with
		| TwinId | RuleId | Value | Type    | StartTime        | EndTime        |
		| TW-12  | Rule-1 | 1.0   | atLeast | <New Start Time> | <New End Time> |

	Then A new command is created with
		| TwinId | RuleId | Value | Type    | StartTime        | EndTime        |
		| TW-12  | Rule-1 | 1.0   | atLeast | <New Start Time> | <New End Time> |

Examples:
	| Existing Start Time | New Start Time | Existing End Time | New End Time    | Status   |
    # New start time is earlier
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 1day    | Pending  |
    # Same start time, end time goes from a specific time to infinity
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      |                 | Pending  |
    # Same start time, new end time is later
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Pending  |
    # New start time is earlier, new end time is later
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Pending  |

    # Approved
    # New start time is earlier
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 1day    | Approved |
    # Same start time, end time goes from a specific time to infinity
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      |                 | Approved |
    # Same start time, new end time is later
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Approved |
    # New start time is earlier, new end time is later
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Approved |

    # Rejected
    # New start time is earlier
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 1day    | Rejected |
    # Same start time, end time goes from a specific time to infinity
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      |                 | Rejected |
    # Same start time, new end time is later
	| TODAY - 1day        | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Rejected |
    # New start time is earlier, new end time is later
	| TODAY - 12hours     | TODAY - 1day   | TODAY + 1day      | TODAY + 36hours | Rejected |

@Unit
Scenario Outline: Command received with different values or different RuleId
	Given I have a command with
		| TwinId | RuleId | Value | Type    | StartTime    | EndTime      | Status  |
		| TW-12  | Rule-1 | 1.0   | exactly | TODAY - 1day | TODAY - 1day | Pending |

	When I receive a command with
		| TwinId | RuleId     | Value       | Type    | StartTime    | EndTime      |
		| TW-12  | <New Rule> | <New Value> | exactly | TODAY - 1day | TODAY - 1day |

	Then A new command is created with
		| TwinId | RuleId     | Value       | Type    | StartTime    | EndTime      |
		| TW-12  | <New Rule> | <New Value> | exactly | TODAY - 1day | TODAY - 1day |

Examples:
	| New Value | New Rule |
	| 1.0       | Rule-2   |
	| 2.0       | Rule-1   |
	| 2.0       | Rule-2   |

@Unit
Scenario: Two commands received in the same batch with different values and command types

	When I receive a command with
		| TwinId | RuleId | Value | Type         | StartTime    | EndTime      |
		| TW-12  | Rule-1 | 1.0   | <New Type 1> | TODAY - 1day | TODAY + 1day |
		| TW-12  | Rule-1 | 2.0   | <New Type 2> | TODAY - 1day | TODAY + 1day |

	Then A new command is created with
		| TwinId | RuleId | Value | Type         | StartTime    | EndTime      |
		| TW-12  | Rule-1 | 1.0   | <New Type 1> | TODAY - 1day | TODAY + 1day |
		| TW-12  | Rule-1 | 2.0   | <New Type 2> | TODAY - 1day | TODAY + 1day |

    Examples:
      | New Type 1 | New Type 2 |
      | atLeast    | atLeast    |
      | atLeast    | atMost     |
      | atLeast    | exactly    |
      | atMost     | atLeast    |
      | atMost     | atMost     |
      | atMost     | exactly    |
      | exactly    | atLeast    |
      | exactly    | atMost     |
      | exactly    | exactly    |

  @Unit
  Scenario: Commands received has invalid connector id, twin id, external id, endtime

    When I receive a command with
      | ExternalId          | ConnectorId          | TwinId          | RuleId | Value | Type         | Start                | End                  |
      | EXT                 | Connector            | TW-12           | Rule-1 | 2.0   | <New Type 2> | 2024-01-01T00:00:00Z | 2024-01-02T00:00:00Z |
      | EXT                 | Connector            | TW-12           | Rule-2 | 2.0   | <New Type 2> | 2024-01-01T00:00:00Z | 1999-01-02T00:00:00Z |
      | EXT                 | Connector            | invalid-twin-id | Rule-3 | 1.0   | <New Type 1> | 2024-01-01T00:00:00Z | 2024-02-02T00:00:00Z |
      | EXT                 | invalid-connector-id | TW-12           | Rule-4 | 1.0   | <New Type 1> | 2024-01-01T00:00:00Z | 2024-02-02T00:00:00Z |
      | invalid-external-id | Connector            | TW-12           | Rule-5 | 1.0   | <New Type 1> | 2024-01-01T00:00:00Z | 2024-02-02T00:00:00Z |

    Then Nothing Happens

    Examples:
      | New Type 1 | New Type 2 |
      | atLeast    | atLeast    |
      | atLeast    | atMost     |
      | atLeast    | exactly    |
      | atMost     | atLeast    |
      | atMost     | atMost     |
      | atMost     | exactly    |
      | exactly    | atLeast    |
      | exactly    | atMost     |
      | exactly    | exactly    |
