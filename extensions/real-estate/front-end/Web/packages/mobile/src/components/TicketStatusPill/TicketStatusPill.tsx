import { styled } from 'twin.macro'
import { Loader, Pill } from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'
import {
  SpecialStatus,
  TICKET_STATUS_DISPLAY_NAMES,
} from '../../utils/ticketStatus'

const StyledPill = styled(Pill)(({ $isHeader }) =>
  $isHeader
    ? {
        textTransform: 'uppercase',
        height: '30px',
        fontWeight: 'var(--font-bold)',
        lineHeight: '30px',
        borderRadius: 0,
        width: '100%',
      }
    : {}
)

/**
 * Ticket status pill box with colour associated to the status.
 * When this is used as a header, we will show it as full width box.
 */
export default function TicketStatusPill({
  className,
  statusCode,
  status,
  isHeader = false,
}: {
  className?: string
  isHeader: boolean
  statusCode?: number
  status?: Status | SpecialStatus
}) {
  const ticketStatuses = useTicketStatuses()
  const ticketStatus =
    status != null
      ? // @ts-expect-error // No idea why this code exits 14 months passed type check previously
        ticketStatuses.getByStatus(status)
      : ticketStatuses.getByStatusCode(statusCode)

  const statusForDisplayName = status || ticketStatus?.status

  return ticketStatuses.isLoading ? (
    <Loader />
  ) : (
    // @ts-expect-error // Types for Pill is having problem, won't bother to fix as it's getting retired soon
    <StyledPill
      className={className}
      $isHeader={isHeader}
      color={ticketStatus?.color || 'red'}
    >
      {statusForDisplayName
        ? TICKET_STATUS_DISPLAY_NAMES[statusForDisplayName]
        : undefined}
    </StyledPill>
  )
}
