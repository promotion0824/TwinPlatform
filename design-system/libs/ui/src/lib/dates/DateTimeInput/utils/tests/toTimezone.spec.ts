import { convertDateWithTimezone } from '../toTimezone'
import { dateStringEquals } from './testUtils'

describe('convertDateWithTimezone', () => {
  it.each([
    [
      new Date(1706706000000),
      'America/Toronto',
      { year: 2024, month: 0, day: 31, hours: 8, minutes: 0 }, // 2024-01-31 08:00
    ],
    [
      new Date(1706706000000),
      'Australia/Sydney',
      { year: 2024, month: 1, day: 1, hours: 0, minutes: 0 }, // 2024-02-01 00:00
    ],
  ])(
    'should convert date to %s local date and time value',
    (date, timezone, expected) => {
      const result = convertDateWithTimezone(date, timezone)
      dateStringEquals(result, expected)
    }
  )
})
