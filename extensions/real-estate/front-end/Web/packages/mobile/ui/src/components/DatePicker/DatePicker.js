import { forwardRef } from 'react'
import { FormControl } from 'components/FormNew/Form'
import DatePicker from './DatePicker/DatePicker'

export { default as DatePickerButton } from './DatePicker/DatePickerButton'

export default forwardRef(function DatePickerComponent(
  { type, children, 'data-segment': dataSegment, ...rest },
  forwardedRef
) {
  const initialValue =
    type === 'date-range' || type === 'date-time-range' ? [] : null

  return (
    <FormControl {...rest} defaultValue={initialValue}>
      {(props) => (
        <DatePicker
          {...props}
          ref={forwardedRef}
          type={type}
          data-segment={dataSegment}
        >
          {children}
        </DatePicker>
      )}
    </FormControl>
  )
})
