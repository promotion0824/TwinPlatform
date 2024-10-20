import cx from 'classnames'
import { Flex, Panel } from '@willow/ui'
import styles from './Panel.css'

export default function PanelComponent({
  isVisible,
  isOpen,
  width = 1200,
  closeWidth = 0,
  children,
  ...rest
}) {
  const cxClassName = cx(styles.panel, {
    [styles.shrinkable]: closeWidth === 0,
  })

  let nextWidth = 0
  if (isVisible) {
    nextWidth = isOpen ? width : closeWidth
  }

  return (
    <Flex className={cxClassName} style={{ width: nextWidth }}>
      <Panel {...rest} className={styles.content}>
        {children}
      </Panel>
    </Flex>
  )
}
