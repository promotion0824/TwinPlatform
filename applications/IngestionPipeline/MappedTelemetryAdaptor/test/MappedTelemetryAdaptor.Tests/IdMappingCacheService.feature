Feature: Id Mapping CacheBackground:
    Scenario: External id is found in the cache
        Given External id is "<externalId>"
        And in the cache external id <externalId> with connector <connectorId> is present
        When it does the look up
        Then it should return <connectorId>
        And metric should be ""

    Examples:
      | externalId                | connectorId               |
      | PNTDXT2JwxMMcS6iDhPZRt4kF | CONRdBLQCNvWoKGUA4kRoUMoe |

    Scenario: External id is not found in the cache
        Given External id is "<externalId>"
        And the cache doesn't have the id
        When it does the look up
        Then it should return 00000000-35c5-4415-a4b3-7b798d0568e8
        And metric should be "1"

    Examples:
      | externalId                |
      | PNTDXT2JwxMMcS6iDhPZRt4kF |

    Scenario: External id is empty
        Given External id is ""
        When it does the look up
        Then it should return 00000000-35c5-4415-a4b3-7b798d0568e8
        And metric should be ""
