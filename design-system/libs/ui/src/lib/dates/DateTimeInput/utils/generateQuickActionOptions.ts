import dayjs from 'dayjs'

type TimeUnit = 'minute' | 'hour' | 'day' | 'week' | 'month' | 'year'

type TimeSpan = {
  unit: TimeUnit
  value: number
}

export type QuickActionOptionShortcut = {
  unit: TimeUnit
  values: TimeSpan['value'][]
  prefix?: 'last'
  suffix?: 'ago'
}

export type QuickActionOption = {
  label: string
  getValue: () => [Date, Date] | Date
}

export const generateQuickActionOptions = (
  optionShortcuts: QuickActionOptionShortcut[]
): QuickActionOption[][] =>
  optionShortcuts.map((optionShortcut) =>
    optionShortcut.values.map((number) => {
      const period = {
        unit: optionShortcut.unit,
        value: number,
      }

      return {
        label: generateLabels({
          period,
          prefix: optionShortcut.prefix,
          suffix: optionShortcut.suffix,
        }),
        getValue: () => getDates(period, Boolean(optionShortcut.prefix)),
      }
    })
  )

export const getDates = (
  period: TimeSpan,
  isRange = false
): [Date, Date] | Date => {
  const dateNow = new Date()
  const dateValue = getPastDate(dateNow, period)
  return isRange ? [dateValue, dateNow] : dateValue
}

export const getPastDate = (currentDate: Date, period: TimeSpan) => {
  const dayjsDate = dayjs(currentDate)

  switch (period.unit) {
    case 'minute':
      return dayjsDate.subtract(period.value, 'minute').toDate()
    case 'hour':
      return dayjsDate.subtract(period.value, 'hour').toDate()
    case 'day':
      return dayjsDate.subtract(period.value, 'day').toDate()
    case 'week':
      return dayjsDate.subtract(period.value, 'week').toDate()
    case 'month':
      return dayjsDate.subtract(period.value, 'month').toDate()
    case 'year':
      return dayjsDate.subtract(period.value, 'year').toDate()
  }
}

export const generateLabels = ({
  period,
  prefix = '',
  suffix = '',
}: {
  period: TimeSpan
  prefix?: string
  suffix?: string
}) => {
  if (period.value === 0 && period.unit === 'day' && suffix === 'ago') {
    return 'today'
  }

  const label = `${prefix} ${period.value} ${pluralize(
    period.unit,
    period.value
  )} ${suffix}`

  return label.trim()
}

export const pluralize = (period: TimeUnit, value: number) => {
  if (value === 1) {
    return period
  }
  return `${period}s`
}
