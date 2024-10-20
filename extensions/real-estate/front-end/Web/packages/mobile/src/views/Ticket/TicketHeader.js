import { Spacing, useDateTime } from '@willow/mobile-ui'
import { useLayout } from '../../providers'
import styles from './TicketHeader.css'
import TicketStatusPill from '../../components/TicketStatusPill/TicketStatusPill.tsx'

export default function TicketHeader({ ticket }) {
  const dateTime = useDateTime()
  const layout = useLayout()

  return (
    <Spacing className={styles.header}>
      <TicketStatusPill isHeader statusCode={ticket.statusCode} />
      <Spacing horizontal align="space middle" className={styles.meta}>
        <div>{ticket.issueName}</div>
        <div className={styles.ticketDate}>
          {dateTime(ticket.createdDate, layout.site?.timeZone).format(
            'dateTimeLong'
          )}
        </div>
      </Spacing>
    </Spacing>
  )
}
