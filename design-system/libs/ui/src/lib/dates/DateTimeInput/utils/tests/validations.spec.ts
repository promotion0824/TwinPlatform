import {
  isDateValid,
  isDateTimeValid,
  isDateRangeValid,
  isDateTimeRangeValid,
} from '../validations'

describe('isDateValid', () => {
  it.each([
    ['date null should be invalid', null, false],
    ['date undefined should be invalid', undefined, false],
    ['date should be valid', new Date(), true],
  ])('%s', (_, date, expected) => {
    expect(isDateValid(date)).toBe(expected)
  })
})

describe('isDateTimeValid', () => {
  const currentDate = new Date()

  it.each([
    ['date null and time undefined should be invalid', null, undefined, false],
    ['date null and valid time should be invalid', null, '08:00', false],

    ['time undefined should be invalid', currentDate, undefined, false],
    ['date and time should be valid', currentDate, '08:00', true],
  ])('%s', (_, date, time, expected) => {
    expect(isDateTimeValid(date, time)).toBe(expected)
  })
})

describe('isDateRangeValid', () => {
  const validStartDate = new Date('2024-01-30')
  const validEndDate = new Date('2024-01-31')
  const invalidEndDate = new Date('2024-01-30')

  it.each([
    ['invalid if start date is null', null, validEndDate, false],
    ['invalid if end date is null', validStartDate, null, false],
    [
      'invalid if start date is after end date',
      validEndDate,
      invalidEndDate,
      false,
    ],
    [
      'valid if start date is before end date',
      validStartDate,
      validEndDate,
      true,
    ],
  ])('%s', (_, startDate, endDate, expected) => {
    expect(isDateRangeValid(startDate, endDate)).toBe(expected)
  })
})

describe('isDateTimeRangeValid', () => {
  const startDate = new Date('2024-01-30')
  const endDate = new Date('2024-01-31')
  const startTime = '08:00'
  const endTime = '16:00'

  it.each([
    [
      'should return false if start date is null',
      null,
      endDate,
      startTime,
      endTime,
      false,
    ],
    [
      'should return false if end date is null',
      startDate,
      null,
      startTime,
      endTime,
      false,
    ],
    [
      'should return false if start time is undefined',
      startDate,
      endDate,
      undefined,
      endTime,
      false,
    ],
    [
      'should return false if end time is undefined',
      startDate,
      endDate,
      startTime,
      undefined,
      false,
    ],
    [
      'should return false if start date and time is after end date and time',
      endDate,
      startDate,
      endTime,
      startTime,
      false,
    ],
    [
      'should return false if start date and time equals end date and time',
      endDate,
      endDate,
      endTime,
      endTime,
      false,
    ],
    [
      'should return true if start date and time is before end date and time',
      startDate,
      endDate,
      startTime,
      endTime,
      true,
    ],
  ])('%s', (_, startDate, endDate, startTime, endTime, expected) => {
    expect(isDateTimeRangeValid(startDate, endDate, startTime, endTime)).toBe(
      expected
    )
  })
})
