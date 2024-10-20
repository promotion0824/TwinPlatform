import cx from 'classnames'
import { useDateTime } from 'hooks'
import Ago from './Ago'
import SecondsAgo from './SecondsAgo'
import Timer from './Timer'
import styles from './Time.css'

export default function Time({
  value,
  completedValue,
  format = 'dateTime',
  color = false,
  className,
}) {
  const dateTime = useDateTime()

  const cxClassName = cx(
    styles.time,
    {
      [styles.color]: color,
    },
    className
  )

  return (
    <span className={cxClassName}>
      {format === 'timer' && (
        <Timer value={value} completedValue={completedValue} />
      )}
      {format === 'secondsAgo' && <SecondsAgo value={value} />}
      {format === 'ago' && <Ago value={value} />}
      {format !== 'timer' &&
        format !== 'ago' &&
        format !== 'secondsAgo' &&
        dateTime(value).format(format)}
    </span>
  )
}
