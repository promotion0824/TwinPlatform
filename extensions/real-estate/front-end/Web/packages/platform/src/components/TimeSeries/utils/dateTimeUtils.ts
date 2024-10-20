/* eslint-disable import/prefer-default-export */
import { DateTime } from 'luxon'

/**
 * Replace the time zone for a date/time, preserving the local date/time
 *
 * This helper will create a date based in nextTimeZone with the date and time
 * (same year, month, day, hour, minute, second and millisecond)
 * from the provided dateTime that is based in a timeZone.
 *
 * For example, provided a DateTime 2020-06-01T12:00:00 in Sydney time,
 * this helper will return a DateTime 2020-06-01T12:00:00 in Hawaii.
 *
 * @param dateTimeStr ISO-8601 compliant format
 * @param timeZone The time zone that the dateTimeStr is based on.
 * If this is not defined, the default zone will be the system's timeZone.
 * @param nextTimeZone The new time zone.  If this is not defined,
 * the default zone will be the system's timeZone
 */
export const replaceTimeZoneForDateTime = (
  dateTimeStr: string,
  timeZone?: string,
  nextTimeZone?: string
) => {
  if (!timeZone && !nextTimeZone) {
    throw new Error('At least one of timeZone and nextTimeZone must be defined')
  }

  return DateTime.fromISO(dateTimeStr, {
    zone: timeZone,
  })
    .setZone(nextTimeZone, { keepLocalTime: true })
    .toUTC()
    .toISO()
}

/**
 * Replace the time zone for a date/time range, preserving the local date/time.
 * @see {retainDateTimeForNextTimeZone}
 */
export const replaceTimeZoneForDateTimeRange = (
  dateTimeRange: [string, string],
  timeZone?: string,
  nextTimeZone?: string
) => {
  const [from, to] = dateTimeRange
  return [
    replaceTimeZoneForDateTime(from, timeZone, nextTimeZone),
    replaceTimeZoneForDateTime(to, timeZone, nextTimeZone),
  ]
}
