import React from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { DateTime } from 'luxon'
import Wrapper from '../../../../utils/testUtils/Wrapper'
import {
  assertDateTimeRangeClose,
  assertDateTimeClose,
} from '../../../../utils/testUtils/date'
import {
  supportDropdowns,
  openDropdown,
} from '../../../../utils/testUtils/dropdown'
import DatePicker from '../DatePicker'

type DatePickerType = 'date' | 'date-time' | 'date-range' | 'date-time-range'

const initialTimezone = 'Australia/Sydney'
const onChange = jest.fn()

const assertCalendarDates = (expectedCalendarStartDate: string) => {
  const datesEl = screen.getByTestId('dates')

  let calendarDate: DateTime = DateTime.fromISO(expectedCalendarStartDate)
  const expectedDays: number[] = []
  for (let i = 0; i < 42; i++) {
    expectedDays.push(calendarDate.day)
    calendarDate = calendarDate.plus({ days: 1 })
  }

  const dateEls = datesEl.querySelectorAll('.date')

  expect(dateEls).toHaveLength(expectedDays.length)

  for (let i = 0; i < expectedDays.length; i++) {
    expect(dateEls[i]).toHaveTextContent(`${expectedDays[i]}`)
  }
}

const assertSelectedDateInCalendar = (
  expectedDate: string,
  type: 'from' | 'to'
) => {
  const datesEl = screen.getByTestId('dates')
  const selectedDateEl = datesEl.querySelectorAll(`.date.${type}`)

  expect(selectedDateEl).toHaveLength(1)
  expect(selectedDateEl[0]).toHaveTextContent(
    `${DateTime.fromISO(expectedDate).day}`
  )
  expect(selectedDateEl[0]).toHaveClass(type)
}

const assertCalendarTime = async (
  expectedTime: [string, string?],
  datePickerType: DatePickerType
) => {
  expect(
    await screen.findByLabelText(
      datePickerType.includes('range') ? 'labels.from' : 'labels.time'
    )
  ).toHaveTextContent(expectedTime[0])
  if (datePickerType.includes('range')) {
    expect(await screen.findByLabelText('labels.to')).toHaveTextContent(
      expectedTime[1] as string
    )
  }
}

const assertOnChangeCall = (
  callIndex: number,
  expectedDateRange: string | [string, string],
  expectedIsCustom: boolean
) => {
  if (typeof expectedDateRange === 'string') {
    assertDateTimeClose(onChange.mock.calls[callIndex][0], expectedDateRange)
  } else {
    assertDateTimeRangeClose(
      onChange.mock.calls[callIndex][0],
      expectedDateRange
    )
  }

  expect(!!onChange.mock.calls[callIndex][1]).toEqual(expectedIsCustom)
}

afterEach(() => {
  onChange.mockClear()
})

describe('DatePicker', () => {
  supportDropdowns()

  describe('date type', () => {
    const type = 'date'
    const initialValue = '2022-06-01'

    test('Show calendar with selected date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022'))

      expect(await screen.findByText('June')).toBeVisible()

      assertCalendarDates('2022-05-30')
      assertSelectedDateInCalendar('2022-06-01', 'from')
    })

    test('Selects another date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022'))

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      userEvent.click(fifth[0])

      // Calendar should be closed.
      expect(screen.queryByTestId('dates')).toBeNull()

      expect(onChange).toBeCalledTimes(1)

      assertOnChangeCall(0, '2022-06-05T00:00:00.000+10:00', false)
    })
  })

  describe('date-time type', () => {
    const type = 'date-time'
    const initialValue = '2022-06-01T15:00:00.000+10:00'

    test('Show Calendar with selected date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022, 15:00'))

      expect(await screen.findByText('June')).toBeVisible()

      assertCalendarDates('2022-05-30')
      assertSelectedDateInCalendar('2022-06-01', 'from')
      await assertCalendarTime(['15:00'], type)
    })

    test('Selects another date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022, 15:00'))

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      // Selecting a date
      userEvent.click(fifth[0])

      expect(onChange).toBeCalledTimes(1)
      assertOnChangeCall(0, '2022-06-05T00:00:00.000+10:00', false)

      // Dropdown should remain open
      expect(await screen.findByTestId('dates')).toBeVisible()
    })

    test('Selects another time', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022, 15:00'))

      // Change time
      openDropdown(await screen.findByText('15:00'))
      userEvent.click(await screen.findByText('12:00'))

      expect(onChange).toBeCalledTimes(1)
      assertOnChangeCall(0, '2022-06-01T12:00:00.000+10:00', false)

      // Dropdown should remain open
      expect(await screen.findByTestId('dates')).toBeVisible()
    })

    test('Changing DatePicker value while Calendar is open should update selection', async () => {
      const { rerender } = render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022, 15:00'))

      rerender(
        <DatePicker
          type={type}
          value="2022-06-20T20:00:00.000+10:00"
          timezone={initialTimezone}
          onChange={onChange}
        />
      )

      assertCalendarDates('2022-05-30')
      assertSelectedDateInCalendar('2022-06-20', 'from')
      await assertCalendarTime(['20:00'], type)
    })

    test('Changing timezone in DatePicker while Calendar is opened should display the dates correctly', async () => {
      const { rerender } = render(
        <DatePicker
          type={type}
          value="2022-08-01T00:00:00.000+10:00"
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Aug 1, 2022, 00:00'))

      rerender(
        <DatePicker
          type={type}
          // Value based on new time zone
          value="2022-08-01T00:00:00.000-10:00"
          timezone="Pacific/Honolulu"
          onChange={onChange}
        />
      )

      assertCalendarDates('2022-08-01')
      assertSelectedDateInCalendar('2022-08-01', 'from')
      await assertCalendarTime(['00:00'], type)
    })

    test('Reset', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022, 15:00'))

      userEvent.click(await screen.findByText('plainText.reset'))

      // Calendar should be closed.
      expect(screen.queryByTestId('dates')).toBeNull()

      expect(onChange).toBeCalledTimes(1)
      expect(onChange).toBeCalledWith(null, undefined)
    })
  })

  describe('date-range type', () => {
    const type = 'date-range'
    const initialValue: [string, string] = ['2022-06-01', '2022-06-15']

    test('Show Calendar with selected date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022 - Jun 15, 2022'))

      expect(await screen.findByText('June')).toBeVisible()

      assertCalendarDates('2022-05-30')
      assertSelectedDateInCalendar('2022-06-01', 'from')
      assertSelectedDateInCalendar('2022-06-15', 'to')
    })

    test('Selects another date range for the same day', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022 - Jun 15, 2022'))

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      // Select from date
      userEvent.click(fifth[0])
      assertSelectedDateInCalendar('2022-06-05', 'from')

      // Select to date
      userEvent.click(fifth[0])

      expect(onChange).toBeCalledTimes(1)
      assertOnChangeCall(
        0,
        ['2022-06-05T00:00:00.000+10:00', '2022-06-05T23:59:59.999+10:00'],
        true
      )
    })

    test('Selects another date range for two different dates', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022 - Jun 15, 2022'))

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      // Selects from date
      userEvent.click(fifth[0])
      assertSelectedDateInCalendar('2022-06-05', 'from')

      // Selects to date
      userEvent.click(fifth[1])

      expect(onChange).toBeCalledTimes(1)
      assertOnChangeCall(
        0,
        ['2022-06-05T00:00:00.000+10:00', '2022-07-05T23:59:59.999+10:00'],
        true
      )
    })

    test('Reset', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(await screen.findByText('Jun 1, 2022 - Jun 15, 2022'))

      userEvent.click(await screen.findByText('plainText.reset'))

      // Calendar should be closed.
      expect(screen.queryByTestId('dates')).toBeNull()

      expect(onChange).toBeCalledTimes(1)
      expect(onChange).toBeCalledWith([], undefined)
    })
  })

  describe('date-time-range type', () => {
    const type = 'date-time-range'
    const initialValue: [string, string] = [
      '2022-06-01T15:00:00.000+10:00',
      '2022-06-15T04:59:59.000+10:00',
    ]

    test('Show Calendar with selected date', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Jun 1, 2022, 15:00 - Jun 15, 2022, 04:59')
      )

      expect(await screen.findByText('June')).toBeVisible()

      assertCalendarDates('2022-05-30')
      assertSelectedDateInCalendar('2022-06-01', 'from')
      assertSelectedDateInCalendar('2022-06-15', 'to')
      await assertCalendarTime(['15:00', '04:59'], type)
    })

    test('Selects another date range for the same day', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Jun 1, 2022, 15:00 - Jun 15, 2022, 04:59')
      )

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      // Selects from date
      userEvent.click(fifth[0])
      assertSelectedDateInCalendar('2022-06-05', 'from')

      // Selects to date
      userEvent.click(fifth[0])

      expect(onChange).toBeCalledTimes(1)

      assertOnChangeCall(
        0,
        ['2022-06-05T00:00:00.000+10:00', '2022-06-05T23:59:59.999+10:00'],
        true
      )

      // Dropdown should remains open
      expect(await screen.findByTestId('dates')).toBeVisible()
    })

    test('Selects another date range for two different dates', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Jun 1, 2022, 15:00 - Jun 15, 2022, 04:59')
      )

      // There are two 5th displayed in the calendar: 5/June and 5/July
      const fifth = await screen.findAllByText('5')
      // Selects from date
      userEvent.click(fifth[0])
      assertSelectedDateInCalendar('2022-06-05', 'from')

      // Selects to date
      userEvent.click(fifth[1])

      expect(onChange).toBeCalledTimes(1)
      assertOnChangeCall(
        0,
        ['2022-06-05T00:00:00.000+10:00', '2022-07-05T23:59:59.999+10:00'],
        true
      )

      // Dropdown should remains open
      expect(await screen.findByTestId('dates')).toBeVisible()
    })

    test('Selects different time range', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Jun 1, 2022, 15:00 - Jun 15, 2022, 04:59')
      )

      // Change from time
      openDropdown(await screen.findByText('15:00'))
      userEvent.click(await screen.findByText('12:00'))

      assertOnChangeCall(
        0,
        ['2022-06-01T12:00:00.000+10:00', '2022-06-15T04:59:59.000+10:00'],
        false
      )

      // Change to time
      openDropdown(await screen.findByText('04:59'))
      userEvent.click(await screen.findByText('13:00'))

      assertOnChangeCall(
        1,
        ['2022-06-01T15:00:00.000+10:00', '2022-06-15T13:00:00.00+10:00'],
        false
      )

      expect(onChange).toBeCalledTimes(2)

      // Dropdown should remains open
      expect(await screen.findByTestId('dates')).toBeVisible()
    })

    test('Reset', async () => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Jun 1, 2022, 15:00 - Jun 15, 2022, 04:59')
      )

      userEvent.click(await screen.findByText('plainText.reset'))

      // Calendar should be closed.
      expect(screen.queryByTestId('dates')).toBeNull()

      expect(onChange).toBeCalledTimes(1)
      expect(onChange).toBeCalledWith([], undefined)
    })

    test("Changing time zone shouldn't affect the dates shown in calendar", async () => {
      const { rerender } = render(
        <DatePicker
          type={type}
          value={[
            '2022-08-01T00:00:00.000+10:00',
            '2022-09-01T00:00:00.000+10:00',
          ]}
          timezone={initialTimezone}
          onChange={onChange}
        />,
        {
          wrapper: Wrapper,
        }
      )

      openDropdown(
        await screen.findByText('Aug 1, 2022, 00:00 - Sep 1, 2022, 00:00')
      )

      rerender(
        <DatePicker
          type={type}
          value={[
            // Update DateTime to be in Pacific/Honolulu timezone.
            '2022-08-01T00:00:00.000-10:00',
            '2022-09-01T00:00:00.000-10:00',
          ]}
          timezone="Pacific/Honolulu"
          onChange={onChange}
        />
      )

      expect(
        screen.queryByText('Aug 1, 2022, 00:00 - Sep 1, 2022, 00:00')
      ).toBeVisible()

      assertCalendarDates('2022-08-01')
      assertSelectedDateInCalendar('2022-08-01', 'from')
      assertSelectedDateInCalendar('2022-09-01', 'to')
      await assertCalendarTime(['00:00', '00:00'], type)
    })

    describe('With quick range options', () => {
      const onSelectQuickRange = jest.fn()

      afterEach(() => {
        onSelectQuickRange.mockClear()
        onChange.mockClear()
      })

      test('Selecting quick range 7D should trigger onChange and onSelectQuickRange correctly', async () => {
        render(
          <DatePicker
            type="date-time-range"
            quickRangeOptions={['7D', 'prevMonth', 'thisMonth']}
            onSelectQuickRange={onSelectQuickRange}
            timezone={initialTimezone}
            onChange={onChange}
            value={[
              '2022-06-01T15:00:00.000+10:00',
              '2022-06-15T04:59:59.000+10:00',
            ]}
            isOuterQuickRangeEnabled
          />,
          { wrapper: Wrapper }
        )

        userEvent.click(await screen.findByText('plainText.7D'))

        expect(onSelectQuickRange).toBeCalledTimes(1)
        expect(onSelectQuickRange).toBeCalledWith('7D')
        expect(onChange).toBeCalledTimes(1)

        assertOnChangeCall(
          0,
          [
            DateTime.now()
              .setZone(initialTimezone)
              .minus({ days: 7 })
              .toISO() as string,
            DateTime.now().setZone(initialTimezone).toISO() as string,
          ],
          true
        )
      })
    })
  })

  describe('Navigating in Calendar', () => {
    const initialValue = '2022-06-01T00:00:00.000+10:00'
    const type = 'date'

    test.each([
      { action: 'prevMonth', expectedStartDate: '2022-04-25' },
      { action: 'nextMonth', expectedStartDate: '2022-06-27' },
      { action: 'prevYear', expectedStartDate: '2021-05-31' },
      { action: 'nextYear', expectedStartDate: '2023-05-29' },
    ])('Click $action', async ({ action, expectedStartDate }) => {
      render(
        <DatePicker
          type={type}
          value={initialValue}
          timezone={initialTimezone}
          onChange={jest.fn}
        />,
        { wrapper: Wrapper }
      )

      openDropdown(await screen.findByText('Jun 1, 2022'))

      userEvent.click(
        screen.getByRole('button', { name: `plainText.${action}` })
      )

      assertCalendarDates(expectedStartDate)
    })
  })
})
