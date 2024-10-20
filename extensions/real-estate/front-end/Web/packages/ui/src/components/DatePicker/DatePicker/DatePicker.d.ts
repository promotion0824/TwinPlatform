import {
  DatePickerBusinessRangeOptions,
  DatePickerDayRangeOptions,
} from '@willow/ui'
import { ComponentProps, PropsWithChildren, ReactElement, Ref } from 'react'
import Dropdown from '../../Dropdown/Dropdown'
import { QuickRangeOption } from './QuickRangeOptions'

type SingleType = 'date' | 'date-time'
type SingleValue = string

type RangeType = 'date-range' | 'date-time-range'
type RangeValue = [string, string]

type GenericDatePickerProps<Type, Value> = ComponentProps<typeof Dropdown> & {
  type: Type
  'data-segment'?: string
  className?: string
  ref?: Ref<HTMLElement>
  /**
   * If specified, prevent the user from selecting a date range that is longer than
   * this many days.
   */
  maxDays?: number
  readOnly?: boolean
  disabled?: boolean
  error?: boolean
  min?: number
  max?: number
  placeholder?: string
  position?: any
  height?: 'large'
  helper?: ReactNode
  value?: Value
  onChange: (value: Value | null, isCustomRange?: boolean) => void
  isOuterQuickRangeEnabled?: boolean
  quickRangeOptions?: QuickRangeOption[]
  selectedQuickRange?: QuickRangeOption
  onSelectQuickRange?: (quickRangeOption: QuickRangeOption) => void
  selectedDayRange?: DatePickerDayRangeOptions
  onDayRangeChange?: (dayRange: DatePickerDayRangeOptions) => void
  onBusinessHourRangeChange?: (
    selectedBusinessHourRange: DatePickerBusinessRangeOptions
  ) => void
  onResetClick?: () => void
  selectedBusinessHourRange?: DatePickerBusinessRangeOptions
  timezone?: string
  timezoneSelector?: ReactElement
  /** Update the z-index of the dropdown content which renders inside a Portal */
  zIndex?: number
}

/**
 * Creates a date picker, time picker, date range picker, or time range picker
 * depending on the `type` prop. This exists for backwards compatibility with
 * the original Javascript version, but `SingleDatePicker` or `DateRangePicker`
 * should be preferred for better type safety.
 */
export default function DatePicker(
  props: PropsWithChildren<
    GenericDatePickerProps<SingleType | RangeType, SingleValue | RangeValue>
  >
): ReactElement

/**
 * Creates a date picker or time picker, depending on the `type` prop.
 */
export function SingleDatePicker(
  props: PropsWithChildren<GenericDatePickerProps<SingleType, SingleValue>>
): ReactElement

/**
 * Creates a date range picker or a time range picker, depending on the `type` prop.
 */
export function DateRangePicker(
  props: PropsWithChildren<GenericDatePickerProps<RangeType, RangeValue>>
): ReactElement
