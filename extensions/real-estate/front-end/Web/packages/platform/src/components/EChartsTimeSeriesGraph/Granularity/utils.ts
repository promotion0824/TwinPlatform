/* eslint-disable no-else-return */
import { DateTime, FixedOffsetZone } from 'luxon'

type DateTimeRange = [from: string, to: string]

export type Interval =
  | { minutes: number }
  | { hours: number }
  | { days: number }
  | { weeks: number }
  | { months: number }

/**
 * Get the possible granularity options (array of interval object) based on the
 * range of days (days difference between two DateTime).
 * The mapping is based on https://dev.azure.com/willowdev/Unified/_workitems/edit/14117
 */
export const getGranularityOptions = (times: DateTimeRange): Interval[] => {
  // Force the times to be interpreted in a fixed time zone, otherwise there
  // might be a daylight savings time change in the middle of the interval
  // which would throw the calculation off by an hour.
  const diffInDays = DateTime.fromISO(times[1], {
    zone: FixedOffsetZone.instance(0),
  }).diff(
    DateTime.fromISO(times[0], { zone: FixedOffsetZone.instance(0) }),
    'days'
  ).days

  if (diffInDays <= 1) {
    return [{ minutes: 5 }, { minutes: 10 }, { minutes: 15 }, { minutes: 30 }]
  } else if (diffInDays <= 3) {
    return [
      { minutes: 5 },
      { minutes: 10 },
      { minutes: 15 },
      { minutes: 30 },
      { hours: 1 },
    ]
  } else if (diffInDays <= 7) {
    return [
      { minutes: 10 },
      { minutes: 15 },
      { minutes: 30 },
      { hours: 1 },
      { hours: 2 },
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
    ]
  } else if (diffInDays <= 11) {
    return [
      { minutes: 15 },
      { minutes: 30 },
      { hours: 1 },
      { hours: 2 },
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
    ]
  } else if (diffInDays <= 35) {
    return [
      { minutes: 30 },
      { hours: 1 },
      { hours: 2 },
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
    ]
  } else if (diffInDays <= 49) {
    return [
      { hours: 1 },
      { hours: 2 },
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
      { weeks: 1 },
    ]
  } else if (diffInDays <= 91) {
    return [
      { hours: 2 },
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
      { weeks: 1 },
    ]
  } else if (diffInDays <= 182) {
    return [
      { hours: 4 },
      { hours: 12 },
      { days: 1 },
      { weeks: 1 },
      { months: 1 },
    ]
  } else {
    return [{ hours: 12 }, { days: 1 }, { weeks: 1 }, { months: 1 }]
  }
}

// Fraction of 15 minutes in a day
const fifteenMinutes = 0.25 / 24

/**
 * Get the default granularity as interval object based on the provided DateTime from-to range.
 * Note: The returned default granularity cannot be more than the range difference.
 *
 * The returned granularity is based on the time range as follows:
 * - For 15 minutes or less => 5 minutes, or
 * - For 7 days or less => 15 minutes, or
 * - For more than 7 days => the first item of the granularity options.
 */
export const getDefaultGranularity = (times: DateTimeRange) => {
  const diffInDays = DateTime.fromISO(times[1]).diff(
    DateTime.fromISO(times[0]),
    'days'
  ).days

  if (diffInDays <= fifteenMinutes) {
    return { minutes: 5 }
  } else if (diffInDays <= 7) {
    return { minutes: 15 }
  } else {
    const granularityOptions = getGranularityOptions(times)
    return granularityOptions[0]
  }
}
