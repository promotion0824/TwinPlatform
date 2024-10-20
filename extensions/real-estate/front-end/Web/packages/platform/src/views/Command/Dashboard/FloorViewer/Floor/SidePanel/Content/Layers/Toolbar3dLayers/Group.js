import { useState } from 'react'
import cx from 'classnames'
import { useEffectOnceMounted } from '@willow/common'
import { useUser, Button, Icon } from '@willow/ui'
import styles from './Group.css'
import VisibleLayersStatus from '../VisibleLayersStatus/VisibleLayersStatus'

export default function Group({
  header,
  visibleLayersCount,
  isReadOnly,
  children,
}) {
  const user = useUser()

  const [isOpen, setIsOpen] = useState(() =>
    header != null ? user.options[`toolbar-group-${header}`] ?? true : true
  )

  useEffectOnceMounted(() => {
    user.saveOptions(`toolbar-group-${header}`, isOpen)
  }, [isOpen])

  const cxClassName = cx(styles.group, {
    [styles.isOpen]: isOpen,
  })

  return (
    <div className={cxClassName}>
      <Button
        className={styles.button}
        onClick={() => setIsOpen((prevIsOpen) => !prevIsOpen)}
      >
        <div className={styles.chevron}>
          <Icon icon="chevronFill" className={styles.chevronIcon} />
        </div>
        <div className={styles.header}>
          {header}
          {isReadOnly && <VisibleLayersStatus number={visibleLayersCount} />}
        </div>
      </Button>
      <div className={styles.content}>{children}</div>
    </div>
  )
}
