import cx from 'classnames'
import { Flex, Portal } from '@willow/ui'
import { useLayout } from './LayoutContext'
import styles from './LayoutHeaderPanel.css'

export default function LayoutHeaderPanel({
  className = undefined,
  children = undefined,
  ...rest
}) {
  const layout = useLayout()

  const cxClassName = cx(styles.layoutHeaderPanel, className)

  return (
    <Portal target={layout.headerPanelRef}>
      <Flex horizontal {...rest} className={cxClassName}>
        {children}
      </Flex>
    </Portal>
  )
}
