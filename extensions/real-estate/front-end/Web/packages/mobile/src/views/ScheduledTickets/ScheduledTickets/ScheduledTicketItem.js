import { useParams } from 'react-router'
import cx from 'classnames'
import tw from 'twin.macro'
import { useDateTime, Spacing, Link } from '@willow/mobile-ui'
import { useTicketStatuses } from '@willow/common'
import { Status, Tab } from '@willow/common/ticketStatus'
import { useLayout } from '../../../providers'
import priorities from '../../../components/priorities.json'
import styles from './ScheduledTicketItem.css'
import TicketProgress from '../Common/TicketProgress'
import TicketStatusPill from '../../../components/TicketStatusPill/TicketStatusPill.tsx'

export default function ScheduledTicketItem({
  id,
  issueName,
  summary,
  dueDate,
  statusCode,
  priority,
  tasks = [],
}) {
  const dateTime = useDateTime()
  const params = useParams()
  const layout = useLayout()
  const ticketStatuses = useTicketStatuses()
  const ticketStatus = ticketStatuses.getByStatusCode(statusCode)

  const isStatusVisible = ticketStatus?.tab === Tab.open
  const cxClassName = cx(styles.ticket, {
    [styles[`priority${priority}`]]: true,
  })

  return (
    <Link
      to={`/sites/${params.siteId}/scheduled-tickets/view/${id}`}
      className={styles.link}
      data-segment="Mobile Ticket Selected"
      data-segment-props={JSON.stringify({
        priority: priorities.find((item) => item.id === priority)?.name,
        status: ticketStatus?.status,
      })}
    >
      <Spacing type="content" size="medium" className={cxClassName}>
        <div className={styles.description}>{summary}</div>
        <div className={styles.title}>{issueName}</div>
        <Spacing horizontal align="middle">
          <TicketStatusPill tw="marginRight[8px]" statusCode={statusCode} />
          {isStatusVisible && (
            <>
              <TicketProgress
                className={styles.progress}
                isOpen={ticketStatus?.status === Status.open}
                tasks={tasks}
              />
              <span className={cx(styles.dueTime, styles.dueTimeMobile)}>
                {`Due: ${dateTime(dueDate, layout.site?.timeZone).format(
                  'dateTimeLong'
                )}`}
              </span>
            </>
          )}
        </Spacing>
        <span className={cx(styles.dueTime, styles.dueTimeWide)}>
          {`Due: ${dateTime(dueDate, layout.site?.timeZone).format(
            'dateTimeLong'
          )}`}
        </span>
      </Spacing>
    </Link>
  )
}
