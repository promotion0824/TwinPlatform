import { useParams } from 'react-router'
import cx from 'classnames'
import { useTicketStatuses } from '@willow/common'
import { Spacing, useDateTime, Link } from '@willow/mobile-ui'
import { useLayout } from '../../providers'
import priorities from '../../components/priorities.json'
import styles from './TicketItem.css'
import TicketStatusPill from '../../components/TicketStatusPill/TicketStatusPill.tsx'

export default function TicketItem({
  id,
  issueName,
  summary,
  createdDate,
  statusCode,
  priority,
}) {
  const dateTime = useDateTime()
  const params = useParams()
  const layout = useLayout()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  const cxClassName = cx(styles.ticket, {
    [styles[`priority${priority}`]]: true,
  })

  return (
    <Link
      to={`/tickets/sites/${params.siteId}/view/${id}`}
      className={styles.link}
      data-segment="Mobile Ticket Selected"
      data-segment-props={JSON.stringify({
        priority: priorities.find((item) => item.id === priority)?.name,
        status: ticketStatus?.status,
      })}
    >
      <Spacing type="content" size="medium" className={cxClassName}>
        <p>
          {dateTime(createdDate, layout.site?.timeZone).format('dateTimeLong')}
        </p>
        <div className={styles.description}>{summary}</div>
        <h5 className={styles.title}>{issueName}</h5>
        <Spacing horizontal align="middle">
          <TicketStatusPill statusCode={statusCode} siteId={params.siteId} />
        </Spacing>
      </Spacing>
    </Link>
  )
}
