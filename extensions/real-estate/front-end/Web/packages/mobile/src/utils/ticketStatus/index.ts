import { Status } from '@willow/common/ticketStatus'

export enum SpecialStatus {
  reject = 'Reject',
}

/**
 * Display names of ticket statuses used throughout the application.
 */
export const TICKET_STATUS_DISPLAY_NAMES = {
  // NOTE: Reject is temporary status in Mobile.
  [SpecialStatus.reject]: 'Reject',
  [Status.open]: 'Open',
  [Status.inProgress]: 'In Progress',
  [Status.limitedAvailability]: 'Partially Fixed',
  [Status.onHold]: 'On Hold',
  /**
   * team had decided to name 'Resolved' => 'Completed' in UI without changing the API or data structure.
   * reference: https://dev.azure.com/willowdev/Unified/_workitems/edit/80429
   */
  [Status.resolved]: 'Completed',
  [Status.closed]: 'Closed',
  [Status.reassign]: 'Reassign',
}

/**
 * Map for standard and scheduled ticket type, consisting of:
 * - Key: Ticket status, and
 * - Value: The list of statuses can be transition to, and whether we display
 *   the list as a collapsible dropdown clickable buttons. If isDropdown is false,
 *   we display the list as a row of buttons.
 */
export const POSSIBLE_STATUS_TRANSITIONS = {
  standard: {
    [Status.open]: {
      isDropdown: false,
      nextStatuses: [SpecialStatus.reject, Status.onHold, Status.inProgress],
    },
    [Status.inProgress]: {
      isDropdown: true,
      nextStatuses: [
        Status.limitedAvailability,
        Status.onHold,
        Status.resolved,
        Status.reassign,
      ],
    },
    [Status.onHold]: {
      isDropdown: true,
      nextStatuses: [Status.open, Status.inProgress],
    },
    [Status.limitedAvailability]: {
      isDropdown: true,
      nextStatuses: [Status.resolved, Status.reassign],
    },
    [Status.reassign]: { isDropdown: false, nextStatuses: [Status.reassign] },
  },
  scheduled: {
    [Status.open]: {
      isDropdown: false,
      nextStatuses: [SpecialStatus.reject, Status.onHold, Status.inProgress],
    },
    [Status.inProgress]: {
      isDropdown: true,
      nextStatuses: [Status.onHold, Status.resolved],
    },
    [Status.limitedAvailability]: {
      isDropdown: true,
      nextStatuses: [Status.resolved],
    },
    [Status.onHold]: {
      isDropdown: true,
      nextStatuses: [Status.open, Status.inProgress],
    },
    [Status.reassign]: { isDropdown: false, nextStatuses: [Status.reassign] },
  },
}

/**
 * The colors of button for the transition status, used with {@link POSSIBLE_STATUS_TRANSITIONS}.
 * These colors are different to the status.color from TicketStatusesProvider.
 */
export const STATUS_BUTTON_COLORS = {
  [SpecialStatus.reject]: 'grey',
  [Status.reassign]: 'blue',
  [Status.limitedAvailability]: 'grey',
  [Status.resolved]: 'blue',
  [Status.inProgress]: 'blue',
  [Status.onHold]: 'grey',
  [Status.open]: 'blue',
}
