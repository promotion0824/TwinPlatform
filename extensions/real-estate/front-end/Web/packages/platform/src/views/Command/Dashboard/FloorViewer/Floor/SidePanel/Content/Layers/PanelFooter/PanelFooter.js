import cx from 'classnames'
import { Flex } from '@willow/ui'
import styles from './PanelFooter.css'

export default function PanelFooter({ className, children, ...rest }) {
  const cxClassName = cx(styles.panelFooter, className)

  return (
    <Flex align="right" {...rest} className={cxClassName}>
      {children}
    </Flex>
  )
}
