import dayjs from 'dayjs'
import utc from 'dayjs/plugin/utc'
import timezone from 'dayjs/plugin/timezone'

import { mockNewDate } from './testUtils'
import {
  QuickActionOption,
  generateQuickActionOptions,
} from '../generateQuickActionOptions'

dayjs.extend(utc)
dayjs.extend(timezone)

// fix timezone for tests
dayjs.tz.setDefault('Australia/Sydney')

// convert Sydney dayjs.Dayjs to Date
const mockedDate = dayjs('2020-01-01 01:01:00').toDate()

function expectLabelAndValueEqual(
  a: QuickActionOption[][],
  b: (Omit<QuickActionOption, 'getValue'> & { value: Date | [Date, Date] })[][]
) {
  const getValueFromA = a.map((options) =>
    options.map((option) => ({
      label: option.label,
      value: option.getValue(),
    }))
  )
  expect(getValueFromA).toEqual(
    expect.arrayContaining(
      b.map((options) =>
        expect.arrayContaining(options.map((option) => option))
      )
    )
  )
}

describe('generateQuickActionOptions', () => {
  beforeAll(() => {
    mockNewDate(mockedDate)
  })

  describe('generate options with ago label', () => {
    it('should generate correct minutes list', () => {
      const options = generateQuickActionOptions([
        {
          unit: 'minute',
          values: [1, 60],
          suffix: 'ago',
        },
      ])

      expectLabelAndValueEqual(options, [
        [
          {
            label: '1 minute ago',
            value: dayjs(mockedDate).subtract(1, 'minute').toDate(),
          },
          {
            label: '60 minutes ago',
            value: dayjs(mockedDate).subtract(60, 'minute').toDate(),
          },
        ],
      ])
    })

    it('should generate correct hours list', () => {
      const options = generateQuickActionOptions([
        {
          unit: 'hour',
          values: [1, 24],
          suffix: 'ago',
        },
      ])
      expectLabelAndValueEqual(options, [
        [
          {
            label: '1 hour ago',
            value: dayjs(mockedDate).subtract(1, 'hour').toDate(),
          },
          {
            label: '24 hours ago',
            value: dayjs(mockedDate).subtract(24, 'hour').toDate(),
          },
        ],
      ])
    })

    it('should generate correct days list', () => {
      const options = generateQuickActionOptions([
        {
          unit: 'day',
          values: [0, 1, 30],
          suffix: 'ago',
        },
      ])
      expectLabelAndValueEqual(options, [
        [
          {
            label: 'today',
            value: dayjs(mockedDate).toDate(),
          },
          {
            label: '1 day ago',
            value: dayjs(mockedDate).subtract(1, 'day').toDate(),
          },
          {
            label: '30 days ago',
            value: dayjs(mockedDate).subtract(30, 'day').toDate(),
          },
        ],
      ])
    })

    it('should generate correct months list', () => {
      const options = generateQuickActionOptions([
        {
          unit: 'month',
          values: [1, 12],
          suffix: 'ago',
        },
      ])
      expectLabelAndValueEqual(options, [
        [
          {
            label: '1 month ago',
            value: dayjs(mockedDate).subtract(1, 'month').toDate(),
          },
          {
            label: '12 months ago',
            value: dayjs(mockedDate).subtract(12, 'month').toDate(),
          },
        ],
      ])
    })

    it('should generate correct years list', () => {
      const options = generateQuickActionOptions([
        {
          unit: 'year',
          values: [1, 10],
          suffix: 'ago',
        },
      ])
      expectLabelAndValueEqual(options, [
        [
          {
            label: '1 year ago',
            value: dayjs(mockedDate).subtract(1, 'year').toDate(),
          },
          {
            label: '10 years ago',
            value: dayjs(mockedDate).subtract(10, 'year').toDate(),
          },
        ],
      ])
    })
  })

  it('should generate options with last label for minutes', () => {
    const options = generateQuickActionOptions([
      {
        unit: 'minute',
        values: [1, 60],
        prefix: 'last',
      },
    ])
    expectLabelAndValueEqual(options, [
      [
        {
          label: 'last 1 minute',
          value: [dayjs(mockedDate).subtract(1, 'minute').toDate(), mockedDate],
        },
        {
          label: 'last 60 minutes',
          value: [
            dayjs(mockedDate).subtract(60, 'minute').toDate(),
            mockedDate,
          ],
        },
      ],
    ])
  })
})
