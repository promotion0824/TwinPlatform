import cx from 'classnames'
import styles from './Badge.css'

export default function Badge({ className, children, ...rest }) {
  const cxClassName = cx(styles.badge, className)

  return (
    <div {...rest} className={cxClassName}>
      {children}
    </div>
  )
}
