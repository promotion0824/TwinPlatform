import { DateTimeType, DateValue } from '../types'
import { stringifyDateTime } from './formatters'

/**
 * Combine the date and time into a certain string format.
 *
 * @example
 * '21 Feb 2024'
 * '21 Feb 2024, 12:00 PM'
 * '21 Feb 2024 - 22 Feb 2024'
 * '21 Feb 2024, 12:00 PM - 22 Feb 2024, 12:00 PM'
 */
export function getDisplayText(
  type: DateTimeType,
  dates: [DateValue] | [DateValue, DateValue] | [],
  times: [string | undefined] | [string | undefined, string | undefined] | []
) {
  if (type === 'date' || type === 'date-time') {
    if (!dates[0]) return ''

    if (type === 'date') {
      return stringifyDateTime(dates[0])
    }

    if (!times[0]) return ''
    return stringifyDateTime(dates[0], times[0])
  }

  if (type === 'date-range' || type === 'date-time-range') {
    if (!dates[0] || !dates[1]) return ''

    if (type === 'date-range') {
      return `${stringifyDateTime(dates[0])} - ${stringifyDateTime(dates[1])}`
    }

    if (!times[0] || !times[1]) return ''
    return `${stringifyDateTime(dates[0], times[0])} - ${stringifyDateTime(
      dates[1],
      times[1]
    )}`
  }

  throw new Error(`Invalid type: ${type}`)
}
