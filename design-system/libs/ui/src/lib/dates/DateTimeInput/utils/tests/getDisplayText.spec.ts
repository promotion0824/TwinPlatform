import { DateTimeType, DateValue } from '../../types'
import { getDisplayText } from '../getDisplayText'

describe('getDisplayText', () => {
  const mockedStartDate = new Date('2021-01-01')
  const mockedStartTime = '08:00'
  const mockedEndDate = new Date('2024-03-01')
  const mockedEndTime = '16:00'

  it.each([
    // 'date'
    [
      'return empty string if no date is provided',
      'date',
      [null],
      [undefined],
      '',
    ],
    [
      'return date string if date is provided',
      'date',
      [mockedStartDate],
      [undefined],
      '01 Jan 2021',
    ],
    [
      'return date string if date and time is provided',
      'date',
      [mockedStartDate],
      [mockedStartTime],
      '01 Jan 2021',
    ],
    // 'date-range'
    [
      'return empty string if no date is provided',
      'date-range',
      [null, null],
      [undefined, undefined],
      '',
    ],
    [
      'return empty string if no end date is provided',
      'date-range',
      [mockedStartDate, null],
      [undefined, undefined],
      '',
    ],
    [
      'return date range string if dates are provided',
      'date-range',
      [mockedStartDate, mockedEndDate],
      [undefined, undefined],
      '01 Jan 2021 - 01 Mar 2024',
    ],
    [
      'return date range string if dates and times are provided',
      'date-range',
      [mockedStartDate, mockedEndDate],
      [mockedStartTime, mockedEndTime],
      '01 Jan 2021 - 01 Mar 2024',
    ],
    // 'date-time'
    [
      'return empty string if no date is provided',
      'date-time',
      [null],
      [undefined],
      '',
    ],
    [
      'return empty string if no time is provided',
      'date-time',
      [mockedStartDate],
      [undefined],
      '',
    ],
    [
      'return date time string if date and time is provided',
      'date-time',
      [mockedStartDate],
      [mockedStartTime],
      '01 Jan 2021, 08:00 AM',
    ],
    // 'date-time-range'
    [
      'return empty string if no date is provided',
      'date-time-range',
      [null, null],
      [undefined, undefined],
      '',
    ],
    [
      'return empty string if no end date is provided',
      'date-time-range',
      [mockedStartDate, null],
      [undefined, undefined],
      '',
    ],
    [
      'return empty string if no time is provided',
      'date-time-range',
      [mockedStartDate, mockedEndDate],
      [undefined, undefined],
      '',
    ],
    [
      'return empty string if no end time is provided',
      'date-time-range',
      [mockedStartDate, mockedEndDate],
      [mockedStartTime, undefined],
      '',
    ],
    [
      'return date time range string if date time and end date time is provided',
      'date-time-range',
      [mockedStartDate, mockedEndDate],
      [mockedStartTime, mockedEndTime],
      '01 Jan 2021, 08:00 AM - 01 Mar 2024, 04:00 PM',
    ],
  ])('%s', (_, type, dates, times, expected) => {
    expect(
      getDisplayText(
        type as DateTimeType,
        dates as [DateValue] | [DateValue, DateValue],
        times as [string | undefined] | [string | undefined, string | undefined]
      )
    ).toEqual(expected)
  })

  it('should throw error for invalid type', () => {
    expect(() =>
      getDisplayText(
        'invalid-type' as DateTimeType,
        [new Date('2024-01-01')],
        [undefined]
      )
    ).toThrow()
  })
})
