import dayjs from 'dayjs'
import timezone from 'dayjs/plugin/timezone'
import utc from 'dayjs/plugin/utc'

import { render, screen, userEvent } from '../../../../../jest/testUtils'

import { QuickActionSelector } from '.'
import { mockNewDate } from '../../utils'

dayjs.extend(utc)
dayjs.extend(timezone)

// fix timezone for tests
dayjs.tz.setDefault('Australia/Sydney')

// convert Sydney dayjs.Dayjs to Date
const mockedDate = dayjs('2020-01-01 01:01:00').toDate()
const mockedHandleSelect = jest.fn()

function clickOptionByText(text: string) {
  const option = screen.getByText(new RegExp(`^\\s*${text}\\s*$`, 'i'))
  return userEvent.click(option)
}
function getPastDateFromMockedDate(value: number, unit: dayjs.ManipulateType) {
  return dayjs(mockedDate).subtract(value, unit).toDate()
}

describe('QuickActionSelector', () => {
  beforeAll(() => {
    mockNewDate(mockedDate)
  })

  beforeEach(() => {
    mockedHandleSelect.mockClear()
  })

  test.each([
    ['Today', mockedDate],
    ['1 day ago', getPastDateFromMockedDate(1, 'day')],
    ['5 days ago', getPastDateFromMockedDate(5, 'day')],
    ['1 week ago', getPastDateFromMockedDate(1, 'week')],
    ['2 weeks ago', getPastDateFromMockedDate(2, 'week')],
    ['1 month ago', getPastDateFromMockedDate(1, 'month')],
    ['2 months ago', getPastDateFromMockedDate(2, 'month')],
    ['1 year ago', getPastDateFromMockedDate(1, 'year')],
    ['2 years ago', getPastDateFromMockedDate(2, 'year')],
  ])(
    'should return correct date when select "%s" with type=date',
    async (label, expectedDate) => {
      render(<QuickActionSelector type="date" onSelect={mockedHandleSelect} />)

      await clickOptionByText(label)

      expect(mockedHandleSelect).toBeCalledWith(expectedDate)
    }
  )

  test.each([
    ['5 minutes ago', getPastDateFromMockedDate(5, 'minute')],
    ['45 minutes ago', getPastDateFromMockedDate(45, 'minute')],
    ['1 hour ago', getPastDateFromMockedDate(1, 'hour')],
    ['2 hours ago', getPastDateFromMockedDate(2, 'hour')],
    ['Today', mockedDate],
    ['1 day ago', getPastDateFromMockedDate(1, 'day')],
    ['5 days ago', getPastDateFromMockedDate(5, 'day')],
    ['1 week ago', getPastDateFromMockedDate(1, 'week')],
    ['2 weeks ago', getPastDateFromMockedDate(2, 'week')],
    ['1 month ago', getPastDateFromMockedDate(1, 'month')],
    ['2 months ago', getPastDateFromMockedDate(2, 'month')],
    ['1 year ago', getPastDateFromMockedDate(1, 'year')],
    ['2 years ago', getPastDateFromMockedDate(2, 'year')],
  ])(
    'should return correct date time when select "%s" with type=date-time',
    async (label, expectedDate) => {
      render(
        <QuickActionSelector type="date-time" onSelect={mockedHandleSelect} />
      )

      await clickOptionByText(label)

      expect(mockedHandleSelect).toBeCalledWith(expectedDate)
    }
  )

  test.each([
    ['last 1 day', getPastDateFromMockedDate(1, 'day')],
    ['last 5 days', getPastDateFromMockedDate(5, 'day')],
    ['last 1 week', getPastDateFromMockedDate(1, 'week')],
    ['last 2 weeks', getPastDateFromMockedDate(2, 'week')],
    ['last 1 month', getPastDateFromMockedDate(1, 'month')],
    ['last 2 months', getPastDateFromMockedDate(2, 'month')],
    ['last 1 year', getPastDateFromMockedDate(1, 'year')],
    ['last 2 years', getPastDateFromMockedDate(2, 'year')],
  ])(
    'should return correct date range when select "%s" with type=date-range',
    async (label, expectedDate) => {
      render(
        <QuickActionSelector type="date-range" onSelect={mockedHandleSelect} />
      )

      await clickOptionByText(label)

      expect(mockedHandleSelect).toBeCalledWith([expectedDate, mockedDate])
    }
  )

  test.each([
    ['last 5 minutes', getPastDateFromMockedDate(5, 'minute')],
    ['last 45 minutes', getPastDateFromMockedDate(45, 'minute')],
    ['last 1 hour', getPastDateFromMockedDate(1, 'hour')],
    ['last 2 hours', getPastDateFromMockedDate(2, 'hour')],
    ['last 1 day', getPastDateFromMockedDate(1, 'day')],
    ['last 5 days', getPastDateFromMockedDate(5, 'day')],
    ['last 1 week', getPastDateFromMockedDate(1, 'week')],
    ['last 2 weeks', getPastDateFromMockedDate(2, 'week')],
    ['last 1 month', getPastDateFromMockedDate(1, 'month')],
    ['last 2 months', getPastDateFromMockedDate(2, 'month')],
    ['last 1 year', getPastDateFromMockedDate(1, 'year')],
    ['last 2 years', getPastDateFromMockedDate(2, 'year')],
  ])(
    'should return correct date time when select "%s" with type=date-time-range',
    async (label, expectedDate) => {
      render(
        <QuickActionSelector
          type="date-time-range"
          onSelect={mockedHandleSelect}
        />
      )

      await clickOptionByText(label)

      expect(mockedHandleSelect).toBeCalledWith([expectedDate, mockedDate])
    }
  )
})
