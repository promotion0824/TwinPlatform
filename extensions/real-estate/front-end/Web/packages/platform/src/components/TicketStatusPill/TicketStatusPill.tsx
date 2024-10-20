import { useTicketStatuses, titleCase } from '@willow/common'
import { Badge, BadgeProps } from '@willowinc/ui'
import { getTicketStatusTranslatedName, Progress } from '@willow/ui'
import { useTranslation } from 'react-i18next'

/**
 * The Pillbox for displaying ticket status. This component support the display of status
 * by either ticket's status or status code.
 */
export default function TicketStatusPill({
  statusCode,
  size = 'md',
  variant = 'dot',
}: {
  statusCode: number
  size?: BadgeProps['size']
  variant?: BadgeProps['variant']
}) {
  const {
    t,
    i18n: { language },
  } = useTranslation()
  const ticketStatuses = useTicketStatuses()

  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  return ticketStatuses.isLoading ? (
    <Progress />
  ) : (
    <Badge
      size={size}
      variant={variant}
      color={ticketStatus ? ticketStatus.color : 'gray'}
    >
      {titleCase({
        text:
          getTicketStatusTranslatedName(t, ticketStatus?.status ?? '') ?? '-',
        language,
      })}
    </Badge>
  )
}
