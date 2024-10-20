import { Spacing, useDateTime } from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status } from '@willow/common/ticketStatus'

import { useLayout } from '../../../providers'
import styles from './TicketHeader.css'
import TicketProgress from '../Common/TicketProgress'
import TicketSection from './TicketSection'
import TicketStatusPill from '../../../components/TicketStatusPill/TicketStatusPill.tsx'

export default function TicketHeader({ ticket }) {
  const { site } = useLayout()
  const dateTime = useDateTime()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(ticket.statusCode)

  return (
    <TicketSection>
      <Spacing>
        <Spacing horizontal align="middle">
          <TicketStatusPill statusCode={ticket.statusCode} />
          <TicketProgress
            className={styles.progress}
            isOpen={ticketStatus?.status === Status.open}
            tasks={ticket.tasks}
          />
        </Spacing>
        <Spacing horizontal align="middle" padding="large 0 0">
          <Spacing>
            <div>{ticket.issueName}</div>
            <div>{`ID ${ticket.id}`}</div>
            <div>{`${dateTime(ticket.createdDate, site?.timeZone).format(
              'dateTimeLong'
            )}`}</div>
          </Spacing>
          <Spacing className={styles.dueByColumn}>
            <div>Due by</div>
            <div className={styles.dueByDate}>
              {`${dateTime(ticket.dueDate, site?.timeZone).format(
                'dateTimeLong'
              )}`}
            </div>
          </Spacing>
        </Spacing>
        <div className={styles.topPartSeparator} />
      </Spacing>
    </TicketSection>
  )
}
