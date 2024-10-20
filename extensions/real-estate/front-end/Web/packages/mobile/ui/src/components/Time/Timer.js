import { useCurrentTime } from 'providers'
import { useDateTime } from 'hooks'
import styles from './Time.css'

export default function Timer(props) {
  const { value, completedValue } = props

  const currentTime = useCurrentTime()
  const dateTime = useDateTime()

  if (value == null) {
    return <span />
  }

  const hours = dateTime(completedValue ?? currentTime).differenceInHours(value)
  const minutes =
    dateTime(completedValue ?? currentTime).differenceInMinutes(value) % 60

  if (hours >= 96) {
    const days = dateTime(completedValue ?? currentTime).differenceInDays(value)

    return (
      <span className={styles.timer}>
        {/* eslint-disable-next-line */}
        {days}d {hours % 24}h
      </span>
    )
  }

  return (
    <span>
      {/* eslint-disable-next-line */}
      {`${hours}`.padStart(2, '0')}:{`${minutes}`.padStart(2, '0')}
    </span>
  )
}
