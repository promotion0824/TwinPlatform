import cx from 'classnames'
import styles from './Table.css'

export default function Body({ className, children, ...rest }) {
  const cxClassName = cx(styles.body, className)

  return (
    <div {...rest} className={cxClassName}>
      {children}
    </div>
  )
}
