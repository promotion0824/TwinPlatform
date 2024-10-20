import cx from 'classnames'
import { Spacing, Icon } from '@willow/mobile-ui'
import styles from './InspectionCheckStatus.css'

export default function InspectionCheckStatus({ checked }) {
  return (
    <Spacing
      type="content"
      align="center middle"
      className={styles.ticketConfirm}
    >
      <div className={cx(styles.iconContainer, { [styles.checked]: checked })}>
        <Icon
          icon="check"
          className={cx(styles.icon, { [styles.checked]: checked })}
        />
      </div>
    </Spacing>
  )
}
