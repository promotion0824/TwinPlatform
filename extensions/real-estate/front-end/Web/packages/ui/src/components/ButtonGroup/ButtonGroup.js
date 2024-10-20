import cx from 'classnames'
import styles from './ButtonGroup.css'

export default function ButtonGroup({ className, children }) {
  const cxClassName = cx(styles.buttonGroup, className)

  return <span className={cxClassName}>{children}</span>
}
