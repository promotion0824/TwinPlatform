import { DateTimeInputValue, DateTimeType } from '../types'
import { getTimeFromDate } from './formatters'
import { getDisplayText } from './getDisplayText'
import { convertDateWithTimezone } from './toTimezone'
import { isDateValid, isValidTimezone } from './validations'

type InitialValues =
  | { dates: []; time: []; inputValue: string }
  | { dates: [Date]; time: [string] | []; inputValue: string }
  | { dates: [Date, Date]; time: [string, string] | []; inputValue: string }
/**
 * Calculates the initial value for DateTimeInput with the value user passed in
 * by its type.
 */
export const calculateInitialValues = <T extends DateTimeType = 'date'>(
  type: T,
  timezone: string,
  value?: DateTimeInputValue<T>
): InitialValues => {
  if (timezone && !isValidTimezone(timezone)) {
    throw new Error('Invalid timezone provided: ' + timezone)
  }

  if (type === 'date' || type === 'date-time') {
    if (!value) {
      return { dates: [], time: [], inputValue: '' }
    }

    if (!(value instanceof Date) || !isDateValid(value)) {
      throw new Error('Invalid date provided: ' + value)
    }

    const dateInTimezone = convertDateWithTimezone(value as Date, timezone)

    const timeString = getTimeFromDate(dateInTimezone)
    return {
      dates: [dateInTimezone] as [Date],
      time: type === 'date-time' ? [timeString] : [],
      inputValue: getDisplayText(type, [dateInTimezone], [timeString]),
    }
  }

  if (type === 'date-range' || type === 'date-time-range') {
    if (!value) {
      return { dates: [], time: [], inputValue: '' }
    }

    if (!Array.isArray(value)) {
      throw new Error('Invalid date range provided: ' + value)
    }

    if (!(value[0] instanceof Date) || !isDateValid(value[0])) {
      throw new Error('Invalid start date provided: ' + value)
    }

    if (!(value[1] instanceof Date) || !isDateValid(value[1])) {
      throw new Error('Invalid end date provided: ' + value)
    }

    const datesInTimezone = (value as [Date, Date]).map((date) =>
      convertDateWithTimezone(date, timezone)
    ) as [Date, Date]

    const timeStrings = datesInTimezone.map(getTimeFromDate) as [string, string]

    return {
      dates: datesInTimezone,
      time: type === 'date-time-range' ? timeStrings : [],
      inputValue: getDisplayText(type, datesInTimezone, timeStrings),
    }
  }

  throw new Error(`Unknown DateTimeType: ${type}`)
}
