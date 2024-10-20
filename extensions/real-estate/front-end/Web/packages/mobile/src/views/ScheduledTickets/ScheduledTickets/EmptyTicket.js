import { Icon, Spacing } from '@willow/mobile-ui'
import styles from './EmptyTicket.css'

export default function EmptyTicket() {
  return (
    <Spacing horizontal type="content" className={styles.empty}>
      <Icon icon="notFound" />
      <div className={styles.message}>There are no tickets</div>
    </Spacing>
  )
}
