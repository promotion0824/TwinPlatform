import { forwardRef, useEffect, useState } from 'react'
import { useDebounce, useForwardedRef } from '@willow/ui'
import TextArea from './TextArea/TextArea'

export default forwardRef(function TextAreaComponent(
  { value, debounce, error, onChange, onFocus, onBlur, ...rest },
  forwardedRef
) {
  const debouncedOnChange = useDebounce(onChange, debounce)

  const textAreaRef = useForwardedRef(forwardedRef)

  const [state, setState] = useState(() => ({
    value,
  }))

  useEffect(() => {
    if (state.value !== value) {
      setState((prevState) => ({
        ...prevState,
        value,
      }))
    }
  }, [value])

  function handleChange(nextValue) {
    setState((prevState) => ({
      ...prevState,
      value: nextValue,
    }))

    const hasFocus = document.activeElement === textAreaRef.current

    if (value !== nextValue) {
      if (hasFocus && debounce != null && error == null) {
        debouncedOnChange?.(nextValue)
      } else {
        onChange?.(nextValue)
      }
    }
  }

  function handleFocus(e) {
    setState((prevState) => ({
      ...prevState,
      value,
    }))

    onFocus?.(e)
  }

  function handleBlur(e) {
    setState((prevState) => ({
      ...prevState,
      value,
    }))

    debouncedOnChange.cancel()

    if (value !== e.currentTarget.value) {
      onChange?.(e.currentTarget.value)
    }

    onBlur?.(e)
  }

  return (
    <TextArea
      {...rest}
      ref={textAreaRef}
      value={state.value ?? ''}
      error={error}
      onChange={handleChange}
      onFocus={handleFocus}
      onBlur={handleBlur}
    />
  )
})
