import { TicketStatusesType } from '@willow/common/providers/TicketStatusesProvider/TicketStatusesProvider'
import { isTicketStatusIncludes, Status } from '@willow/common/ticketStatus'
import { useDateTime } from '@willow/ui'
import { TicketSimpleDto } from '../../services/Tickets/TicketsService'
import { DueBy } from './ticketsProviderTypes'

const filterTicketByDueDate = (
  ticket: TicketSimpleDto,
  dueBy: DueBy,
  dateTime: ReturnType<typeof useDateTime>,
  ticketStatus: TicketStatusesType
) =>
  isTicketActive(ticket.statusCode, ticketStatus) &&
  filterByDueDate(ticket.dueDate, dueBy, dateTime)

const filterByDueDate = (
  dueDate: string | null | undefined,
  dueBy: DueBy,
  dateTime: ReturnType<typeof useDateTime>
): boolean => {
  if (!dueDate) return false

  // The same logic extract from the original OverduePill,
  // which uses endOfDay() to make sure the dueDate is the end of the day
  const differenceInMilliseconds = dateTime(dueDate)
    .endOfDay()
    .differenceInMilliseconds(dateTime.now())
  const differenceInDays = dateTime(dueDate)
    .endOfDay()
    .differenceInDays(dateTime.now())

  if (dueBy === DueBy.Overdue) {
    return differenceInMilliseconds <= 0
  }

  if (dueBy === DueBy.Today) {
    return differenceInMilliseconds > 0 && differenceInDays === 0
  }

  if (dueBy === DueBy.Next7Days) {
    return differenceInMilliseconds > 0 && differenceInDays <= 7
  }

  if (dueBy === DueBy.Next30Days) {
    return differenceInMilliseconds > 0 && differenceInDays <= 30
  }

  return false
}

/** Resolved and Closed tickets are not considered Active tickets */
const isTicketActive = (
  statusCode: number,
  ticketStatuses: TicketStatusesType
) => {
  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  return (
    !!ticketStatus &&
    !isTicketStatusIncludes(ticketStatus, [Status.resolved, Status.closed])
  )
}

export default filterTicketByDueDate
