import cx from 'classnames'
import styles from './Table.css'

export default function Head({ className, children, ...rest }) {
  const cxClassName = cx(styles.head, className)

  return (
    <div {...rest} className={cxClassName}>
      {children}
    </div>
  )
}
