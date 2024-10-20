/* eslint-disable @typescript-eslint/no-non-null-assertion */
import { fireEvent, render } from '@testing-library/react'
import { DateTime } from 'luxon'
import QuickRangeOptions, { QuickRangeOption } from '../QuickRangeOptions'
import { assertDateTimeRangeClose } from '../../../../utils/testUtils/date'
import Wrapper from '../../../../utils/testUtils/Wrapper'

const onSelectQuickRange = jest.fn()

const assertOnSelectQuickRangeArgs = (
  expectedSelected: QuickRangeOption,
  expectedDateRange: [string, string]
) => {
  expect(onSelectQuickRange).toBeCalledTimes(1)

  const [selected, dateTimeRange] = onSelectQuickRange.mock.calls[0]

  expect(selected).toBe(expectedSelected)
  assertDateTimeRangeClose(dateTimeRange, expectedDateRange)
}

beforeEach(() => {
  onSelectQuickRange.mockClear()
})

describe('QuickRangeOptions', () => {
  test('Selects quick range option of: 7D', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['7D', '1M', '3M', '6M', '1Y']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.7D/))

    assertOnSelectQuickRangeArgs('7D', [
      DateTime.now().minus({ days: 7 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option of: 1M', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['7D', '1M', '3M', '6M', '1Y']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.oneMonthShort/))

    assertOnSelectQuickRangeArgs('1M', [
      DateTime.now().minus({ days: 30 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option of: 3M', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['7D', '1M', '3M', '6M', '1Y']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.threeMonthsShort/))

    assertOnSelectQuickRangeArgs('3M', [
      DateTime.now().minus({ days: 90 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option of: 6M', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['7D', '1M', '3M', '6M', '1Y']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.sixMonthsShort/))

    assertOnSelectQuickRangeArgs('6M', [
      DateTime.now().minus({ days: 180 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option of: 1Y', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['7D', '1M', '3M', '6M', '1Y']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.oneYearShort/))

    assertOnSelectQuickRangeArgs('1Y', [
      DateTime.now().minus({ days: 365 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option: This Month', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['thisMonth', 'prevMonth']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const today = DateTime.now()

    fireEvent.click(getByText(/plainText.thisMonth/))

    assertOnSelectQuickRangeArgs('thisMonth', [
      today.startOf('month').toISO()!,
      today.endOf('month').toISO()!,
    ])
  })

  test('Selects quick range option of: Prev. Month', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['thisMonth', 'prevMonth']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )
    const lastMonth = DateTime.now().minus({ month: 1 })

    fireEvent.click(getByText(/plainText.prevMonth/))

    assertOnSelectQuickRangeArgs('prevMonth', [
      lastMonth.startOf('month').toISO()!,
      lastMonth.endOf('month').toISO()!,
    ])
  })

  test('Selects quick range option of: 24H', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['24H', '48H']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.24H/))

    assertOnSelectQuickRangeArgs('24H', [
      DateTime.now().minus({ days: 1 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })

  test('Selects quick range option of: 48H', () => {
    const { getByText } = render(
      <QuickRangeOptions
        options={['24H', '48H']}
        onSelect={onSelectQuickRange}
      />,
      {
        wrapper: Wrapper,
      }
    )

    fireEvent.click(getByText(/plainText.48H/))

    assertOnSelectQuickRangeArgs('48H', [
      DateTime.now().minus({ days: 2 }).toISO()!,
      DateTime.now().toISO()!,
    ])
  })
})
