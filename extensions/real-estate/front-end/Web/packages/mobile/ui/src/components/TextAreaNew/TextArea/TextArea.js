import { forwardRef, useState } from 'react'
import cx from 'classnames'
import { useDebounce } from 'hooks'
import styles from './TextArea.css'

export default forwardRef(function TextArea(
  {
    value,
    disabled,
    error,
    readOnly,
    debounce,
    className,
    style,
    onChange,
    onFocus,
    onBlur,
    ...rest
  },
  forwardedRef
) {
  const [state, setState] = useState({
    value,
    hasFocus: false,
  })

  const debouncedOnChange = useDebounce(
    onChange,
    debounce === true ? 300 : debounce
  )

  const formattedValue = (state.hasFocus ? state.value : value) ?? ''

  const cxClassName = cx(
    styles.textArea,
    {
      [styles.hasValue]: value?.length > 0,
      [styles.isDisabled]: disabled || readOnly,
      [styles.hasError]: error != null,
    },
    className
  )

  function handleChange(e) {
    const nextValue = e.currentTarget.value

    setState((prevState) => ({
      ...prevState,
      value: nextValue,
    }))

    if (!debounce) {
      onChange(nextValue)
    }

    if (debounce) {
      debouncedOnChange(nextValue)
    }
  }

  function handleFocus(e) {
    setState((prevState) => ({
      ...prevState,
      value,
      hasFocus: true,
    }))

    onFocus?.(e)
  }

  function handleBlur(e) {
    setState((prevState) => ({
      ...prevState,
      hasFocus: false,
    }))

    if (debounce) {
      debouncedOnChange.cancel()

      if (e.currentTarget.value !== value) {
        onChange(e.currentTarget.value)
      }
    }

    onBlur?.(e)
  }

  return (
    <span className={cxClassName} style={style} ref={forwardedRef}>
      <textarea
        maxLength={2000}
        rows={6}
        data-error={error != null ? true : undefined}
        {...rest}
        value={formattedValue}
        readOnly={readOnly}
        className={styles.textAreaControl}
        onChange={handleChange}
        onFocus={handleFocus}
        onBlur={handleBlur}
      />
    </span>
  )
})
