import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'

import { DateTimeType, DateValue } from '../types'
import { compareDates } from './compareDates'
import { combineDateAndTimeString } from './getOutputValues'

dayjs.extend(utc)
dayjs.extend(timezone)

export function checkIfDateTimeValid(
  type: DateTimeType,
  selectedDates: [DateValue] | [DateValue, DateValue] | [],
  selectedTime:
    | [string | undefined]
    | [string | undefined, string | undefined]
    | []
) {
  switch (type) {
    case 'date':
      return isDateValid(selectedDates[0])
    case 'date-time':
      return isDateTimeValid(selectedDates[0], selectedTime[0])
    case 'date-range':
      return isDateRangeValid(selectedDates[0], selectedDates[1])
    case 'date-time-range':
      return isDateTimeRangeValid(
        selectedDates[0],
        selectedDates[1],
        selectedTime[0],
        selectedTime[1]
      )
    default:
      throw new Error(`Invalid type: ${type}`)
  }
}

export function checkIfTimezoneValid(
  timezone?: string | null
): timezone is string {
  return !!timezone && isValidTimezone(timezone)
}

export function isDateValid(date?: DateValue): date is Date {
  return date != null
}

export function isDateTimeValid(date?: DateValue, time?: string): date is Date {
  return isDateValid(date) && !!time
}

export function isDateRangeValid(startDate?: DateValue, endDate?: DateValue) {
  return (
    isDateValid(startDate) &&
    isDateValid(endDate) &&
    compareDates(startDate, endDate) === -1
  )
}

export function isDateTimeRangeValid(
  startDate?: DateValue,
  endDate?: DateValue,
  startTime?: string,
  endTime?: string
) {
  return (
    isDateTimeValid(startDate, startTime) &&
    isDateTimeValid(endDate, endTime) &&
    startTime &&
    endTime &&
    compareDates(
      combineDateAndTimeString(startDate, startTime),
      combineDateAndTimeString(endDate, endTime)
    ) === -1
  )
}

export const isValidTimezone = (timezoneName: string) => {
  try {
    const testDate = dayjs.tz(new Date(), timezoneName)
    return testDate.isValid() && testDate.format('Z') !== 'Invalid Date'
  } catch (error) {
    return false
  }
}
