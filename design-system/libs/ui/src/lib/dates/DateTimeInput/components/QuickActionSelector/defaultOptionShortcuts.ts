import { DateTimeType } from '../../types'
import {
  QuickActionOptionShortcut,
  generateQuickActionOptions,
} from '../../utils/generateQuickActionOptions'

const defaultMinuteValues = [5, 15, 30, 45]
const defaultHourValues = [1, 2, 3, 4, 6, 12]
const defaultDayValues = [1, 2, 3, 4, 5, 6]
const defaultWeekValues = [1, 2, 3, 4]
const defaultMonthValues = [1, 2, 6]
const defaultYearValues = [1, 2]

export const getOptionShortcutsByType = (type: DateTimeType) => {
  switch (type) {
    case 'date':
      return dateOptionShortcut
    case 'date-time':
      return dateTimeOptionShortcut
    case 'date-range':
      return dateRangeOptionShortcut
    case 'date-time-range':
      return dateTimeRangeOptionShortcut
  }
}

export const getDefaultOptionsByType = (type: DateTimeType) =>
  generateQuickActionOptions(getOptionShortcutsByType(type))

const dateOptionShortcut: QuickActionOptionShortcut[] = [
  {
    unit: 'day',
    values: [0, ...defaultDayValues],
    suffix: 'ago',
  },
  {
    unit: 'week',
    values: defaultWeekValues,
    suffix: 'ago',
  },
  {
    unit: 'month',
    values: defaultMonthValues,
    suffix: 'ago',
  },
  {
    unit: 'year',
    values: defaultYearValues,
    suffix: 'ago',
  },
]

const dateTimeOptionShortcut: QuickActionOptionShortcut[] = [
  {
    unit: 'minute',
    values: defaultMinuteValues,
    suffix: 'ago',
  },
  {
    unit: 'hour',
    values: defaultHourValues,
    suffix: 'ago',
  },
  {
    unit: 'day',
    values: [0, ...defaultDayValues],
    suffix: 'ago',
  },
  {
    unit: 'week',
    values: defaultWeekValues,
    suffix: 'ago',
  },
  {
    unit: 'month',
    values: defaultMonthValues,
    suffix: 'ago',
  },
  {
    unit: 'year',
    values: defaultYearValues,
    suffix: 'ago',
  },
]
const dateRangeOptionShortcut: QuickActionOptionShortcut[] = [
  {
    unit: 'day',
    values: defaultDayValues,
    prefix: 'last',
  },
  {
    unit: 'week',
    values: defaultWeekValues,
    prefix: 'last',
  },
  {
    unit: 'month',
    values: defaultMonthValues,
    prefix: 'last',
  },
  {
    unit: 'year',
    values: defaultYearValues,
    prefix: 'last',
  },
]

const dateTimeRangeOptionShortcut: QuickActionOptionShortcut[] = [
  {
    unit: 'minute',
    values: defaultMinuteValues,
    prefix: 'last',
  },
  {
    unit: 'hour',
    values: defaultHourValues,
    prefix: 'last',
  },
  {
    unit: 'day',
    values: defaultDayValues,
    prefix: 'last',
  },
  {
    unit: 'week',
    values: defaultWeekValues,
    prefix: 'last',
  },
  {
    unit: 'month',
    values: defaultMonthValues,
    prefix: 'last',
  },
  {
    unit: 'year',
    values: defaultYearValues,
    prefix: 'last',
  },
]
