import { useHistory } from 'react-router'
import { Tab } from '@willow/common/ticketStatus'
import { Spacing, Icon, Link, useTimeout } from '@willow/mobile-ui'

import styles from './TicketConfirm.css'

export default function TicketConfirm({
  title = 'Ticket Completed',
  subTitle = 'This ticket was saved successfully',
  siteId,
  enableNavigation = true,
}) {
  const history = useHistory()

  const navToHome = () => {
    if (enableNavigation) {
      history.push(`/tickets/sites/${siteId}/${Tab.open}`)
    }
  }

  useTimeout(navToHome, 3000)

  return (
    <Spacing
      type="content"
      align="center middle"
      className={styles.ticketConfirm}
    >
      <div className={styles.container}>
        <div className={styles.iconContainer}>
          <Icon icon="check" className={styles.icon} />
        </div>
        <h3 className={styles.title}>{title}</h3>
        <p className={styles.subTitle}>{subTitle}</p>
        <Link
          to={`/tickets/sites/${siteId}/${Tab.open}`}
          className={styles.link}
        >
          Go to tickets
        </Link>
      </div>
    </Spacing>
  )
}
