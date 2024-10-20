import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import styles from './FloatingPanelFooter.css'

export default function FloatingPanelFooter({ className, children, ...rest }) {
  const cxClassName = cx(styles.floatingPanelFooter, className)

  return (
    <Flex align="right" {...rest} className={cxClassName}>
      {children}
    </Flex>
  )
}
