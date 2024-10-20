import { Spacing, Icon } from '@willow/mobile-ui'

import styles from './TicketSection.css'

export default function TicketSection({ icon, title, children }) {
  return (
    <Spacing horizontal type="content" size="medium" className={styles.section}>
      <div className={styles.icon}>
        <Icon icon={icon} size="small" />
      </div>
      <Spacing type="content" size="medium" className={styles.content}>
        <h4 className={styles.title}>{title}</h4>
        <div>{children}</div>
      </Spacing>
    </Spacing>
  )
}
