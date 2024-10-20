import { useHistory } from 'react-router'
import { Tab } from '@willow/common/ticketStatus'
import cx from 'classnames'
import { Spacing, Icon, Link, useTimeout } from '@willow/mobile-ui'

import styles from './TicketConfirm.css'

export default function TicketConfirm({
  title = 'Ticket Completed',
  subTitle = 'This ticket was saved successfully',
  siteId,
  enableNavigation = true,
  showReassignVariant,
}) {
  const history = useHistory()
  const icon = showReassignVariant ? 'back' : 'check'
  const ticketConfirmClassName = cx(styles.ticketConfirm, {
    [styles.reassignVariant]: showReassignVariant,
  })

  const navToHome = () => {
    if (enableNavigation) {
      history.push(`/sites/${siteId}/scheduled-tickets/${Tab.open}`)
    }
  }

  useTimeout(navToHome, 3000)

  return (
    <Spacing
      type="content"
      align="center middle"
      className={ticketConfirmClassName}
    >
      <div className={styles.container}>
        <div className={styles.iconContainer}>
          <Icon icon={icon} className={styles.icon} />
        </div>
        <div className={styles.title}>{title}</div>
        <p className={styles.subTitle}>{subTitle}</p>
        <Link
          to={`/sites/${siteId}/scheduled-tickets/${Tab.open}`}
          className={styles.link}
        >
          Go to tickets
        </Link>
      </div>
    </Spacing>
  )
}
