import { forwardRef } from 'react'
import { useDateTime, useForwardedRef } from 'hooks'
import Dropdown, { DropdownContent } from '../../DropdownNew/Dropdown'
import Text from '../../Text/Text'
import { DatePickerContext } from './DatePickerContext'
import Calendar from './Calendar'
import KeyboardHandler from './KeyboardHandler'

export default forwardRef(function DatePicker(
  {
    type = 'date',
    value,
    readOnly,
    disabled,
    error,
    min,
    max,
    placeholder,
    helper,
    position,
    height,
    className,
    onChange,
    'data-segment': dataSegment,
    ...rest
  },
  forwardedRef
) {
  const dateTime = useDateTime()
  const dropdownRef = useForwardedRef(forwardedRef)
  let derivedValue = value
  if (type === 'date' || type === 'date-time') {
    derivedValue = [value]
  }

  function getFormattedValue(nextValue) {
    let format = 'date'
    if (type === 'date-time' || type === 'date-time-range') format = 'date-time'

    let formattedValue = dateTime(nextValue[0]).format(format)
    if (formattedValue == null) {
      return ''
    }
    if (
      (type === 'date-range' || type === 'date-time-range') &&
      nextValue.length === 2
    ) {
      formattedValue = `${formattedValue} - ${dateTime(nextValue[1]).format(
        format
      )}`
    }

    return formattedValue ?? ''
  }

  const formattedValue = getFormattedValue(derivedValue)
  const hasValue = formattedValue !== ''
  const nextPlaceholder = placeholder != null ? `- ${placeholder} -` : undefined
  const hasPlaceholder = !hasValue && nextPlaceholder != null

  const context = {
    type,
    value: derivedValue,
    min,
    max,
    helper,

    select(date, isCustomRange) {
      onChange(date, isCustomRange)
      dropdownRef.current.focus()
    },
  }

  return (
    <DatePickerContext.Provider value={context}>
      <Dropdown
        {...rest}
        icon="calendar"
        ref={dropdownRef}
        readOnly={readOnly}
        disabled={disabled}
        error={error}
        position={position}
        data-segment={dataSegment}
      >
        <Text>{hasPlaceholder ? nextPlaceholder : formattedValue}</Text>
        <DropdownContent position="bottom">
          <Calendar />
          <KeyboardHandler />
        </DropdownContent>
      </Dropdown>
    </DatePickerContext.Provider>
  )
})
