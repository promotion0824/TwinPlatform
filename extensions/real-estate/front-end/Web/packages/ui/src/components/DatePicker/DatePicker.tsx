import React, { forwardRef, ComponentProps, Ref } from 'react'
import { useIntl } from '../../providers'
import FormControl from '../Form/FormControl'
import * as baseModule from './DatePicker/DatePicker'

export { default as DatePickerButton } from './DatePicker/DatePickerButton'

function DatePickerComponent(
  {
    type,

    timezone = undefined,
    'data-segment': dataSegment = undefined,

    ...rest
  }: {
    type:
      | 'date'
      | 'date-time'
      | 'date-range'
      | 'date-time-range'
      | 'date-business-range'
    timezone?: string
    'data-segment'?: string
  },
  forwardedRef: Ref<HTMLElement>
) {
  const intl = useIntl()

  const initialValue =
    type === 'date-range' || type === 'date-time-range' ? [] : null

  return (
    <FormControl {...rest} defaultValue={initialValue}>
      {(props) => (
        <baseModule.default
          {...props}
          ref={forwardedRef}
          type={type}
          timezone={timezone ?? intl?.timezone}
          data-segment={dataSegment}
        />
      )}
    </FormControl>
  )
}

// These three components share the same implementation but accept different
// prop types. See DatePicker.d.ts.

const DatePicker = forwardRef<
  HTMLElement,
  ComponentProps<typeof baseModule.default>
>(DatePickerComponent)
export default DatePicker

const SingleDatePicker = forwardRef<
  HTMLElement,
  ComponentProps<typeof baseModule.SingleDatePicker>
>(DatePickerComponent)
const DateRangePicker = forwardRef<
  HTMLElement,
  ComponentProps<typeof baseModule.DateRangePicker>
>(DatePickerComponent)

export { SingleDatePicker, DateRangePicker }
