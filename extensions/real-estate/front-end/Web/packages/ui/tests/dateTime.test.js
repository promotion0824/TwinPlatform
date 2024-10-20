import { DateTime } from 'luxon'
import dateTime from '../src/hooks/useDateTime/dateTime'

describe('dateTime', () => {
  test('addMilliseconds, addSeconds, addMinutes, addHours, addDays, addWeeks, addMonths, addYears', () => {
    expect(dateTime('2019-08-01T00:00:00Z').addMilliseconds(1).format()).toBe(
      '2019-08-01T00:00:00.001Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addSeconds(1).format()).toBe(
      '2019-08-01T00:00:01.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addMinutes(1).format()).toBe(
      '2019-08-01T00:01:00.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addHours(1).format()).toBe(
      '2019-08-01T01:00:00.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addDays(1).format()).toBe(
      '2019-08-02T00:00:00.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addWeeks(1).format()).toBe(
      '2019-08-08T00:00:00.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addMonths(1).format()).toBe(
      '2019-09-01T00:00:00.000Z'
    )
    expect(dateTime('2019-08-01T00:00:00Z').addYears(1).format()).toBe(
      '2020-08-01T00:00:00.000Z'
    )
  })

  test('differenceInMilliseconds, differenceInSeconds, differenceInMinutes, differenceInHours, differenceInDays, differenceInWeeks, differenceInMonths, differenceInYears', () => {
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMilliseconds(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(34357057778)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMilliseconds(
        dateTime('2019-10-05T12:34:56.789Z')
      )
    ).toBe(34357057778)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInSeconds(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(34357057)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInSeconds(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBe(34357057.778)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMinutes(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(572617)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMinutes(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBe(572617.6296333333)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInHours(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(9543)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInHours(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBe(9543.627160555556)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInDays(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(397)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInDays(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBeCloseTo(397.6927983564815, 0)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInWeeks(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(56)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInWeeks(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBeCloseTo(56.81325690806878, 0)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMonths(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(13)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInMonths(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBeCloseTo(13.023093278549382, 0)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInYears(
        '2019-10-05T12:34:56.789Z'
      )
    ).toBe(1)
    expect(
      dateTime('2020-11-06T04:12:34.567Z').differenceInYears(
        '2019-10-05T12:34:56.789Z',
        { decimals: true }
      )
    ).toBeCloseTo(1.0868295845383054, 0)
    expect(
      dateTime('2019-10-05T12:34:56.789Z').differenceInYears(
        '2020-11-06T04:12:34.567Z'
      )
    ).toBe(-1)
    expect(
      dateTime('2019-10-05T12:34:56.789Z').differenceInYears(
        '2019-11-05T12:34:56.789Z'
      )
    ).toBe(0)
    expect(dateTime().differenceInYears('2019-11-05T12:34:56.789Z')).toBe(null)
  })

  test('endOfSecond, endOfMinute, endOfHour, endOfDay, endOfWeek, endOfMonth, endOfYear', () => {
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfSecond()
        .format('timezone')
    ).toBe('2019-10-05T23:00:00.999+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfMinute()
        .format('timezone')
    ).toBe('2019-10-05T23:00:59.999+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfHour()
        .format('timezone')
    ).toBe('2019-10-05T23:59:59.999+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfDay()
        .format('timezone')
    ).toBe('2019-10-05T23:59:59.999+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfWeek()
        .format('timezone')
    ).toBe('2019-10-06T23:59:59.999+11:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfMonth()
        .format('timezone')
    ).toBe('2019-10-31T23:59:59.999+11:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .endOfYear()
        .format('timezone')
    ).toBe('2019-12-31T23:59:59.999+11:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Europe/Oslo')
        .endOfWeek()
        .format('timezone')
    ).toBe('2019-10-06T23:59:59.999+02:00')
  })

  test('format', () => {
    function myDateTime(arg) {
      return dateTime(
        DateTime.fromISO(arg, { zone: 'Australia/Sydney' }).toMillis()
      )
    }
    expect(myDateTime('2019').format('timezone', 'Australia/Sydney')).toBe(
      '2019-01-01T00:00:00.000+11:00'
    )
    expect(myDateTime('2019-08').format('timezone', 'Australia/Sydney')).toBe(
      '2019-08-01T00:00:00.000+10:00'
    )
    expect(
      myDateTime('2019-08-10').format('timezone', 'Australia/Sydney')
    ).toBe('2019-08-10T00:00:00.000+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00Z').format('timezone', 'Australia/Sydney')
    ).toBe('2019-08-10T20:00:00.000+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00.0Z').format(
        'timezone',
        'Australia/Sydney'
      )
    ).toBe('2019-08-10T20:00:00.000+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00.1Z').format(
        'timezone',
        'Australia/Sydney'
      )
    ).toBe('2019-08-10T20:00:00.100+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00.12Z').format(
        'timezone',
        'Australia/Sydney'
      )
    ).toBe('2019-08-10T20:00:00.120+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00.123Z').format(
        'timezone',
        'Australia/Sydney'
      )
    ).toBe('2019-08-10T20:00:00.123+10:00')
    expect(
      myDateTime('2019-08-10T10:00:00.1234Z').format(
        'timezone',
        'Australia/Sydney'
      )
    ).toBe('2019-08-10T20:00:00.123+10:00')

    expect(myDateTime('2019-08-10T00:00:00Z').format('utc')).toBe(
      '2019-08-10T00:00:00.000Z'
    )
    expect(myDateTime('2019-08-10T00:00:00Z').format('UTC')).toBe(
      '2019-08-10T00:00:00.000Z'
    )
    expect(
      myDateTime('2019-08-10T00:00:00Z').format('timezone', 'Europe/Oslo')
    ).toBe('2019-08-10T02:00:00.000+02:00')
    expect(
      myDateTime('2019-08-10T00:00:00Z').format(
        'date',
        'Australia/Sydney',
        'en-AU'
      )
    ).toBe('10 Aug 2019')
    expect(
      myDateTime('2019-08-10T00:00:00Z').format(
        'date',
        'America/Phoenix',
        'en-US'
      )
    ).toBe('Aug 9, 2019')
    expect(
      myDateTime('2019-08-10T00:00:00Z').format('date', 'Europe/Paris', 'fr-FR')
    ).toBe('10 août 2019')

    // Note: packages/ui/tests/dateTime.test.js and packages/mobile/ui/tests/dateTime.test.js are identical
    // except for the expected value of this assertion.
    expect(
      myDateTime('2019-08-10T10:00:00Z').format(
        'dateTime',
        'Australia/Sydney',
        'en-AU'
      )
    ).toBe('10 Aug 2019, 20:00')

    expect(
      myDateTime('2019-08-10T10:00:00Z').format(
        'time',
        'Australia/Sydney',
        'en-AU'
      )
    ).toBe('20:00')

    expect(
      myDateTime('2019-08-10T00:00:00Z').format(
        {
          day: 'numeric',
          month: 'numeric',
        },
        'Australia/Sydney',
        'en-AU'
      )
    ).toBe('10/8')

    // invalid time
    expect(dateTime().format()).toBe(null)
    expect(dateTime().format(null)).toBe(null)
    expect(dateTime().format('Invalid')).toBe(null)
  })

  test('get millisecond, second, minute, hour, day, week, month, year', () => {
    expect(
      dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').millisecond
    ).toBe(0)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').second).toBe(0)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').minute).toBe(0)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').day).toBe(1)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').hour).toBe(10)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').week).toBe(31)
    expect(dateTime('2019-08-01T00:00:00Z', 'Australia/Sydney').month).toBe(8)
    expect(dateTime('2019-07-31T20:00:00Z', 'Australia/Sydney').month).toBe(8)
    expect(dateTime('2018-12-31T20:00:00Z', 'Australia/Sydney').year).toBe(2019)
    expect(dateTime('2019-01-01T00:00:00Z', 'Australia/Sydney').year).toBe(2019)
  })

  test('isBetween', () => {
    const time = '2019-10-05T13:00:00Z'
    expect(
      dateTime(time).isBetween('2019-10-05T11:00:00Z', '2019-10-05T13:00:00Z')
    ).toBe(true)
    expect(
      dateTime(time).isBetween('2019-10-05T11:00:00Z', '2019-10-05T12:00:00Z')
    ).toBe(false)
    expect(
      dateTime(time) >= dateTime('2019-10-05T11:00:00Z') &&
        dateTime(time) <= dateTime('2019-10-05T13:00:00Z')
    ).toBe(true)
    expect(
      dateTime(time) >= dateTime('2019-10-05T11:00:00Z') &&
        dateTime(time) <= dateTime('2019-10-05T12:00:00Z')
    ).toBe(false)
  })

  test('isEqual', () => {
    expect(
      dateTime('2019-08-10T10:00:00Z').isEqual('2019-08-10T12:00:00.000+02:00')
    ).toBe(true)
    expect(
      dateTime('2019-08-10T10:00:00.000+02:00').isEqual(
        '2019-08-10T12:00:00.000+02:00'
      )
    ).toBe(false)
    expect(
      +dateTime('2019-08-10T10:00:00Z') ===
        +dateTime('2019-08-10T12:00:00.000+02:00')
    ).toBe(true)
    expect(
      +dateTime('2019-08-10T10:00:00.000+02:00') ===
        +dateTime('2019-08-10T12:00:00.000+02:00')
    ).toBe(false)
  })

  test('setTime', () => {
    expect(
      dateTime('2019-10-17T00:00:00.000+11:00', 'Australia/Sydney')
        .setTime('00:15')
        .format('timezone')
    ).toBe('2019-10-17T00:15:00.000+11:00')
    expect(
      dateTime('2019-10-17T12:34:56.789+11:00', 'Australia/Sydney')
        .setTime('00:15')
        .format('timezone')
    ).toBe('2019-10-17T00:15:00.000+11:00')
    expect(
      dateTime('2019-10-17T00:00:00.000+11:00', 'Australia/Sydney')
        .setTime('23:00')
        .format('timezone')
    ).toBe('2019-10-17T23:00:00.000+11:00')
    expect(
      dateTime('', 'Australia/Sydney').setTime('00:15').format('timezone')
    ).toBe(null)
  })

  test('startOfMillisecond, startOfSecond, startOfMinute, startOfHour, startOfDay, startOfWeek, startOfMonth, startOfYear', () => {
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfMillisecond()
        .format('timezone')
    ).toBe('2019-10-05T23:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfSecond()
        .format('timezone')
    ).toBe('2019-10-05T23:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfMinute()
        .format('timezone')
    ).toBe('2019-10-05T23:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfHour()
        .format('timezone')
    ).toBe('2019-10-05T23:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfDay()
        .format('timezone')
    ).toBe('2019-10-05T00:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfWeek()
        .format('timezone')
    ).toBe('2019-09-30T00:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfMonth()
        .format('timezone')
    ).toBe('2019-10-01T00:00:00.000+10:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Australia/Sydney')
        .startOfYear()
        .format('timezone')
    ).toBe('2019-01-01T00:00:00.000+11:00')
    expect(
      dateTime('2019-10-05T13:00:00Z', 'Europe/Oslo')
        .startOfWeek()
        .format('timezone')
    ).toBe('2019-09-30T00:00:00.000+02:00')
  })

  test('valueOf', () => {
    expect(dateTime('2019-08-01T00:00:00Z').valueOf()).toBe(1564617600000)
    expect(dateTime().valueOf()).toBe(undefined)
  })
})
