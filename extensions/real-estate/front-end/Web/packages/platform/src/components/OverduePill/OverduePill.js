import { useTicketStatuses } from '@willow/common'
import { useDateTime } from '@willow/ui'
import { Badge } from '@willowinc/ui'
import { useTranslation } from 'react-i18next'
import filterTicketByDueDate from '../TicketsNew/filterByDueDate'

export default function OverduePill({ ticket }) {
  const { t } = useTranslation()
  const ticketStatuses = useTicketStatuses()
  const dateTime = useDateTime()

  const isOverdue = filterTicketByDueDate(
    ticket,
    'overdue',
    dateTime,
    ticketStatuses
  )

  if (!isOverdue) {
    return null
  }

  return (
    <Badge variant="outline" size="md" color="red">
      {t('plainText.overdue')}
    </Badge>
  )
}
