import { Spacing } from '@willow/mobile-ui'
import cx from 'classnames'
import styles from './TicketTopBar.css'

export default function TicketTopBar({ ticket }) {
  const priorityClassName = cx(styles.main, {
    [styles[`priority${ticket?.priority}`]]: true,
  })

  return (
    <Spacing className={styles.header}>
      <div className={priorityClassName} />
    </Spacing>
  )
}
