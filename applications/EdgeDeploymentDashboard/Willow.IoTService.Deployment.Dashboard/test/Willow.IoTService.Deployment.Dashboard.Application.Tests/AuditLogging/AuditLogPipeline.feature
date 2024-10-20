@auditlogpipeline
Feature: Audit Logging
    In order to verify that all user actions are tracked
    As a user with permissions
    I want to ensure that every action I perform is logged appropriately

Scenario Outline: User action is logged with permissions
    Given the user has permission
    When the user performs action '<Action>'
    Then the action is logged via IAuditLogger

    Examples: 
      | Action  |
      | Create  |
      | Update  |
      | Delete  |

Scenario Outline: User action is not logged when an exception is thrown
    Given the user has permission
    When the user performs action '<Action>' and it throws an exception
    Then the action is not logged via IAuditLogger
    And an exception is thrown

    Examples: 
      | Action  |
      | Create  |
      | Update  |
      | Delete  |
