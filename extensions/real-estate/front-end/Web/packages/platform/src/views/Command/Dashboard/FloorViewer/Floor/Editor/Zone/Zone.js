import cx from 'classnames'
import styles from './Zone.css'

export default function Zone({ zone, className, ...rest }) {
  const cxClassName = cx(styles.zone, className)

  return (
    <polygon points={zone.points.join(' ')} {...rest} className={cxClassName} />
  )
}
