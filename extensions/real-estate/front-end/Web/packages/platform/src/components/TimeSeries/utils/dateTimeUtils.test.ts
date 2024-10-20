import { DateTime } from 'luxon'
import { replaceTimeZoneForDateTimeRange } from './dateTimeUtils'

describe('replaceTimeZoneForDateTime', () => {
  test('Throw an error when both timeZone and nextTimeZone are not defined', () => {
    expect(() =>
      replaceTimeZoneForDateTimeRange(
        ['2022-06-01T00:00:00.000Z', '2022-06-15T01:22:33.456Z'],
        undefined,
        undefined
      )
    ).toThrowError('At least one of timeZone and nextTimeZone must be defined')
  })

  describe.each([
    {
      description: 'With +10:00 offset',
      dateRange: ['2022-06-01T10:00:00+10:00', '2022-06-15T11:22:33.456+10:00'],
      timeZone: 'Australia/Sydney',
    },
    {
      description:
        'UTC of [2022-06-01T10:00:00+10:00, 2022-06-15T11:22:33.456+10:00]',
      dateRange: ['2022-06-01T00:00:00.000Z', '2022-06-15T01:22:33.456Z'],
      timeZone: 'Australia/Sydney',
    },
  ])('DateRange $dateRange - $description', ({ dateRange, timeZone }) => {
    test.each([
      {
        nextTimeZone: undefined,
        // UTC of [2022-06-01T10:00:00, 2022-06-15T11:22:33.456] in system's timezone
        expectedDateRange: [
          DateTime.fromISO('2022-06-01T10:00:00').toUTC().toISO(),
          DateTime.fromISO('2022-06-15T11:22:33.456').toUTC().toISO(),
        ],
      },
      {
        nextTimeZone: 'Pacific/Honolulu',
        // UTC of [2022-06-01T10:00:00, 2022-06-15T11:22:33.456] in Honolulu timezone
        expectedDateRange: [
          '2022-06-01T20:00:00.000Z',
          '2022-06-15T21:22:33.456Z',
        ],
      },
    ])(
      `Replace timeZone (${timeZone}) with $nextTimeZone`,
      ({ nextTimeZone, expectedDateRange }) => {
        expect(
          replaceTimeZoneForDateTimeRange(
            dateRange as [string, string],
            timeZone,
            nextTimeZone
          )
        ).toEqual(expectedDateRange)
      }
    )
  })
})
