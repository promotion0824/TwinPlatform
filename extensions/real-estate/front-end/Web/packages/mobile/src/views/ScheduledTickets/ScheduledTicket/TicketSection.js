import cx from 'classnames'
import { Spacing, Icon } from '@willow/mobile-ui'

import styles from './TicketSection.css'

export default function TicketSection({
  icon,
  iconSize = 'medium',
  title,
  children,
  className,
}) {
  return (
    <Spacing size="medium" className={cx(styles.section, className)}>
      <Spacing horizontal size="medium" width="100%">
        {icon && (
          <div className={styles.icon}>
            <Icon icon={icon} size={iconSize} />
          </div>
        )}
        {title && <div className={styles.title}>{title}</div>}
      </Spacing>
      <div>{children}</div>
    </Spacing>
  )
}
