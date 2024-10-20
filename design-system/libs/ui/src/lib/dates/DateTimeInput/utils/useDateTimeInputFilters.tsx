import { useCallback, useMemo, useState } from 'react'
import { find } from 'lodash'

import { Select, SelectItem } from '../../../inputs/Select'

const DAYS_DATA: SelectItem[] = [
  { label: 'All days', value: 'allDays' },
  { label: 'Weekdays', value: 'weekDays' },
  { label: 'Weekends', value: 'weekEnds' },
]
const HOURS_DATA: SelectItem[] = [
  { label: 'All hours', value: 'allHours' },
  { label: 'During business hours', value: 'inBusinessHours' },
  { label: 'Outside business hours', value: 'outBusinessHours' },
]

/**
 * Enable filters for DateTimeInput. It is designed to be
 * fixed selectors at the moment. But can be refactored to be configurable
 * and more customizable in the future.
 */
export const useDateTimeInputExtension = ({
  hideDaysFilter = false,
  hideHoursFilter = false,
  initialDaysFilter = 'allDays',
  initialHoursFilter = 'allHours',
  daysFilterLabel = 'Days filter',
  hoursFilterLabel = 'Hours filter',
  daysData = DAYS_DATA,
  hoursData = HOURS_DATA,
}: {
  hideDaysFilter?: boolean
  hideHoursFilter?: boolean
  initialDaysFilter?: string | null
  initialHoursFilter?: string | null
  daysFilterLabel?: string
  hoursFilterLabel?: string
  daysData?: SelectItem[]
  hoursData?: SelectItem[]
} = {}) => {
  const [daysFilter, setDaysFilter] = useState<string | null>(
    hideDaysFilter ? null : initialDaysFilter
  )
  const [hoursFilter, setHoursFilter] = useState<string | null>(
    hideHoursFilter ? null : initialHoursFilter
  )
  const [tempDaysFilter, setTempDaysFilter] = useState<string | null>(
    daysFilter
  )
  const [tempHoursFilter, setTempHoursFilter] = useState<string | null>(
    hoursFilter
  )

  const onCancel = useCallback(() => {
    setTempDaysFilter(initialDaysFilter)
    setTempHoursFilter(initialHoursFilter)
  }, [initialDaysFilter, initialHoursFilter])

  const onApply = useCallback(() => {
    setDaysFilter(tempDaysFilter)
    setHoursFilter(tempHoursFilter)
  }, [tempDaysFilter, tempHoursFilter])

  const onValidate = useCallback(() => {
    return (
      (hideDaysFilter || !!tempDaysFilter) &&
      (hideHoursFilter || !!tempHoursFilter)
    )
  }, [hideDaysFilter, hideHoursFilter, tempDaysFilter, tempHoursFilter])

  const customizeSection = useMemo(
    () => (
      <>
        {/*
         * Important: you need to set `comboboxProps={{ withinPortal: false }}`
         * when using `Select` within `DateTimeInput`.
         * And also considering to have `allowDeselect={false}` to require
         * a valid selection.
         */}
        {!hideDaysFilter && (
          <Select
            allowDeselect={false}
            comboboxProps={{ withinPortal: false }}
            label={daysFilterLabel}
            data={daysData}
            value={tempDaysFilter}
            onChange={setTempDaysFilter}
          />
        )}

        {!hideHoursFilter && (
          <Select
            allowDeselect={false}
            comboboxProps={{ withinPortal: false }}
            label={hoursFilterLabel}
            data={hoursData}
            value={tempHoursFilter}
            onChange={setTempHoursFilter}
          />
        )}
      </>
    ),
    [
      daysData,
      daysFilterLabel,
      hideDaysFilter,
      hideHoursFilter,
      hoursData,
      hoursFilterLabel,
      tempDaysFilter,
      tempHoursFilter,
    ]
  )

  const customizeValueText = useCallback(
    (resultText: string) => {
      const daysLabel = findOptionByValue(daysData, tempDaysFilter)?.label
      const hoursLabel = findOptionByValue(hoursData, tempHoursFilter)?.label

      return `${resultText}${
        hideDaysFilter ? '' : getAppendingText(daysLabel)
      }${hideHoursFilter ? '' : getAppendingText(hoursLabel)}`
    },
    [
      daysData,
      hideDaysFilter,
      hideHoursFilter,
      hoursData,
      tempDaysFilter,
      tempHoursFilter,
    ]
  )

  return {
    // so that unused props won't pass to DateTimeInput component
    ...(hideDaysFilter ? {} : { daysFilter }),
    ...(hideHoursFilter ? {} : { hoursFilter }),
    ...(hideDaysFilter && hideHoursFilter
      ? {}
      : {
          customizeValueText,
          customizeSection,

          onValidate,
          onCancel,
          onApply,
        }),
  }
}

const findOptionByValue = (options: SelectItem[], value: string | null) => {
  return find<SelectItem>(options, { value: value ?? '' })
}

const getAppendingText = (text?: string) => (text ? `, ${text}` : '')
