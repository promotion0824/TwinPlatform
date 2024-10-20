import { getDate, parseDateWithTimezone } from '../getOutputValues'

describe('parseDateWithTimezone', () => {
  it.each([
    ['2024-01-31 08:00:00', 'America/Toronto', 1706706000000],
    ['2024-02-01 00:00', 'Australia/Sydney', 1706706000000],
    ['2024-01-31 21:00', 'Australia/Perth', 1706706000000],
  ])(
    'should return desired Date for %s in timezone %s',
    (date, timezone, expectedTimestamp) => {
      const timeStamp = parseDateWithTimezone(date, timezone).getTime()
      expect(timeStamp).toEqual(expectedTimestamp)
    }
  )
})

describe('getDate', () => {
  it.each([
    [new Date('2024-01-31'), '08:00:00', 'America/Toronto', 1706706000000],
    [new Date('2024-02-01'), '00:00', 'Australia/Sydney', 1706706000000],
    [new Date('2024-01-31'), '21:00', 'Australia/Perth', 1706706000000],
    [new Date('31 January 2024'), '21:00', 'Australia/Perth', 1706706000000],
  ])(
    'should return desired Date for %s in timezone %s',
    (date, time, timezone, expectedTimestamp) => {
      const timeStamp = getDate(date, time, timezone).getTime()
      expect(timeStamp).toEqual(expectedTimestamp)
    }
  )
})
