import dayjs from 'dayjs'
import timezone from 'dayjs/plugin/timezone'
import utc from 'dayjs/plugin/utc'

import { isEmpty, zipWith } from 'lodash'

import { DateTimeInputValue, DateTimeType } from '../types'

dayjs.extend(utc)
dayjs.extend(timezone)

export const parseDateWithTimezone = (dateString: string, timezone: string) =>
  dayjs.tz(dateString, timezone).toDate()

export const combineDateAndTimeString = (date: Date, time: string) =>
  `${dayjs(date).format(
    'YYYY-MM-DD' /* has to be this format, dayjs timezone will parse as wrong Date
     with other format */
  )} ${time}`

export const getDate = (date: Date, time: string, timezone: string) =>
  parseDateWithTimezone(combineDateAndTimeString(date, time), timezone)

type DateValue<T extends DateTimeType> = T extends 'date' | 'date-time'
  ? [Date]
  : T extends 'date-range' | 'date-time-range'
  ? [Date, Date]
  : never
type TimeValue<T extends DateTimeType> = T extends 'date' | 'date-time'
  ? [string]
  : T extends 'date-range' | 'date-time-range'
  ? [string, string]
  : never

/** Generate the output date value for DateTimeInput component. */
export function getOutputDates<T extends DateTimeType>({
  type,
  date,
  time,
  timezone,
}: {
  type: T
  date: DateValue<T>
  time: TimeValue<T> | []
  timezone: string
}): DateTimeInputValue<T> {
  if (type === 'date-range' || type === 'date-time-range') {
    // Handle range types
    const dates = date as [Date, Date]
    const times = isEmpty(time)
      ? ['00:00', '23:59'] /* for DateRange who does not have times selected */
      : time
    return zipWith(dates, times, (date, time) =>
      getDate(date, time, timezone)
    ) as DateTimeInputValue<T>
  }

  // Handle single date types
  const d = date[0] as Date
  const t = time[0] ?? '00:00' /* for Date who does not have time selected */
  return getDate(d, t, timezone) as DateTimeInputValue<T>
}
