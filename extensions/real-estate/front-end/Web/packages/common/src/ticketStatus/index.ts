import _ from 'lodash'
import { TicketStatus, Status, Tab } from './types'

export * from './types'
export { default as useGetTicketStatusesByCustomerId } from '../providers/TicketStatusesProvider/useGetTicketStatusesByCustomerId'

/**
 * Whether a ticket status is equivalent to a provided status.
 * For "closed" and "resolved" status, we check if the ticket status belongs to the
 * closed/resolved tab. For all other statuses, we do an equality check.
 */
export const isTicketStatusEquates = (
  ticketStatus: TicketStatus,
  status: Status
) => {
  if (status === Status.resolved) {
    return ticketStatus.tab === Tab.resolved
  } else if (status === Status.closed) {
    return ticketStatus.tab === Tab.closed
  } else {
    return status === ticketStatus.status
  }
}

/**
 * Whether a list of statuses contains the equivalent ticket status provided.
 * See {@link isTicketStatusEquates}.
 */
export const isTicketStatusIncludes = (
  ticketStatus: TicketStatus,
  statuses: Status[]
) => statuses.some((status) => isTicketStatusEquates(ticketStatus, status))
