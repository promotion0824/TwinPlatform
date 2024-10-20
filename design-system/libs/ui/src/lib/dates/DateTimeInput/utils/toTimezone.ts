import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'
import { isValidTimezone } from './validations'

dayjs.extend(utc)
dayjs.extend(timezone)

export const convertDateWithTimezone = (date: Date, timezone: string) => {
  if (!isValidTimezone(timezone)) {
    throw new Error(`Cannot convert to invalid timezone: ${timezone}`)
  }
  const targetZoneDate = dayjs(date.getTime()).tz(timezone)
  const targetZoneDateValue = targetZoneDate.format('YYYY-MM-DD')
  const targetZoneTimeValue = targetZoneDate.format('HH:mm:ss')
  return new Date(`${targetZoneDateValue}T${targetZoneTimeValue}`)
}
