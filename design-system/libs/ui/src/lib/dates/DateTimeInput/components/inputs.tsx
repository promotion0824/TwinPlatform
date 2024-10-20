import { DateInput, DateInputProps } from '../../DateInput'
import { TimeInput, TimeInputProps } from '../../TimeInput'
import { DateValue } from '../types'
import { dateFormat, timeFormat } from '../utils'

export const SingleDateInput = ({
  label,
  selectedDate = null,
  onDateChange,
  ...props
}: {
  label: string
  selectedDate?: DateValue
  onDateChange: (date: DateValue) => void
} & Partial<DateInputProps>) => (
  <DateInput
    label={label}
    value={selectedDate}
    onChange={onDateChange}
    popoverProps={{ opened: false }}
    valueFormat={dateFormat}
    {...props}
  />
)

export const SingleTimeInput = ({
  label,
  time,
  onTimeChange,
  ...restProps
}: {
  label: string
  time?: string
  onTimeChange: (args: string | undefined) => void
} & Partial<TimeInputProps>) => (
  <TimeInput
    popoverProps={{ withinPortal: false }}
    format={timeFormat}
    label={label}
    value={time}
    onChange={onTimeChange}
    {...restProps}
  />
)
