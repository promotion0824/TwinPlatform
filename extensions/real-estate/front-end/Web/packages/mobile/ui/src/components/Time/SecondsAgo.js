import { useCurrentTime } from 'providers'
import { useDateTime } from 'hooks'

export default function SecondsAgo({ value }) {
  const currentTime = useCurrentTime()
  const dateTime = useDateTime()

  if (value == null) {
    return <span />
  }

  const years = Math.abs(dateTime(value).differenceInYears(currentTime))
  const months = Math.abs(dateTime(value).differenceInMonths(currentTime))
  const days = Math.abs(dateTime(value).differenceInDays(currentTime))
  const hours = Math.abs(dateTime(value).differenceInHours(currentTime))
  const minutes = Math.abs(dateTime(value).differenceInMinutes(currentTime))

  let ms = dateTime(value).differenceInMinutes(currentTime)
  const showNegative = ms <= 0
  ms = Math.abs(ms)

  let str
  if (years > 0) str = `${years}y`
  else if (months > 0) str = `${months}mo`
  else if (days > 0) str = `${days}d`
  else if (hours > 0) str = `${hours}h`
  else str = `${minutes}m`

  if (showNegative) str = `${str} ago`
  else str = `${str} left`

  return <span>{str}</span>
}
