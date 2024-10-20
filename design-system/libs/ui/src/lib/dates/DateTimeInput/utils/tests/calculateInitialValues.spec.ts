import { calculateInitialValues } from '../calculateInitialValues'
import { dateStringEquals } from './testUtils'

describe('calculateInitialValues', () => {
  const timezone = 'America/Toronto'

  const expectedStartTimeValue = '08:00'
  const expectedStartDate = {
    year: 2024,
    month: 0,
    day: 31,
    hours: 8,
    minutes: 0,
  } // 2024-01-31 08:00
  const startDate = new Date(1706706000000)
  const expectedStartDateValue = '31 Jan 2024'
  const expectedStartValue = `${expectedStartDateValue}, 08:00 AM`

  const endDate = new Date(1709505291000)
  const expectedEndDate = {
    year: 2024,
    month: 2,
    day: 3,
    hours: 17,
    minutes: 34,
  } // 2024-03-03 17:34
  const expectedEndTimeValue = '17:34'
  const expectedEndDateValue = '03 Mar 2024'
  const expectedEndValue = `${expectedEndDateValue}, 05:34 PM`

  describe('when type = "date"', () => {
    it('should return empty dates and time and undefined inputValue when no value is provided', () => {
      expect(calculateInitialValues('date', timezone)).toEqual({
        dates: [],
        time: [],
        inputValue: '',
      })
    })
    it('should return date and time and inputValue when value is provided', () => {
      const expectedInputDateValue = '31 Jan 2024'
      const result = calculateInitialValues('date', timezone, startDate) as {
        dates: [Date]
        time: [string] | []
        inputValue: string
      }

      dateStringEquals(result.dates[0], expectedStartDate)
      expect(result.time).toEqual([])
      expect(result.inputValue).toEqual(expectedInputDateValue)
    })

    it('should throw if invalid date is provided', () => {
      expect(() =>
        calculateInitialValues(
          'date',
          timezone,
          '2024-03-01' as unknown as Date
        )
      ).toThrow()
    })
  })

  describe('when type = "date-time"', () => {
    it('should return date and time and inputValue when value is provided', () => {
      const result = calculateInitialValues(
        'date-time',
        timezone,
        startDate
      ) as {
        dates: [Date]
        time: [string] | []
        inputValue: string
      }

      dateStringEquals(result.dates[0], expectedStartDate)
      expect(result.time[0]).toEqual(expectedStartTimeValue)
      expect(result.inputValue).toEqual(expectedStartValue)
    })
  })

  describe('when type = "date-range"', () => {
    it('should return date and inputValue when dates are provided', () => {
      const result = calculateInitialValues('date-range', timezone, [
        startDate,
        endDate,
      ]) as {
        dates: [Date, Date]
        time: [string, string] | []
        inputValue: string
      }

      dateStringEquals(result.dates[0], expectedStartDate)
      dateStringEquals(result.dates[1], expectedEndDate)
      expect(result.time).toEqual([])
      expect(result.inputValue).toEqual(
        `${expectedStartDateValue} - ${expectedEndDateValue}`
      )
    })
  })

  describe('when type = "date-time-range"', () => {
    it('should return date and inputValue when dates are provided', () => {
      const result = calculateInitialValues('date-time-range', timezone, [
        startDate,
        endDate,
      ]) as {
        dates: [Date, Date]
        time: [string, string] | []
        inputValue: string
      }

      dateStringEquals(result.dates[0], expectedStartDate)
      dateStringEquals(result.dates[1], expectedEndDate)
      expect(result.time).toEqual([
        expectedStartTimeValue,
        expectedEndTimeValue,
      ])
      expect(result.inputValue).toEqual(
        `${expectedStartValue} - ${expectedEndValue}`
      )
    })
  })
})
