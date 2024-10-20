import { forwardRef, useLayoutEffect, useState } from 'react'
import cx from 'classnames'
import { useForwardedRef } from '@willow/ui'
import styles from './TextArea.css'

export default forwardRef(function TextArea(
  {
    value,
    error,
    icon,
    readOnly,
    disabled,
    className,
    textAreaClassName,
    onChange,
    ...rest
  },
  forwardedRef
) {
  const [hasValue, setHasValue] = useState(false)

  const textAreaRef = useForwardedRef(forwardedRef)

  useLayoutEffect(() => {
    if (textAreaRef.current) {
      const nextHasValue = textAreaRef.current.value.length > 0

      if (hasValue !== nextHasValue) {
        setHasValue(nextHasValue)
      }
    }
  }, [hasValue, textAreaRef, value])

  function handleChange(e) {
    const nextHasValue = e.currentTarget.value.length > 0
    if (hasValue !== nextHasValue) {
      setHasValue(nextHasValue)
    }

    onChange(e.currentTarget.value)
  }

  const cxClassName = cx(
    styles.textArea,
    {
      [styles.readOnly]: readOnly,
      [styles.disabled]: disabled,
      [styles.hasValue]: hasValue,
      [styles.hasError]: error != null,
    },
    className
  )
  const cxTextAreaClassName = cx(styles.textAreaControl, textAreaClassName)

  return (
    <span className={cxClassName}>
      <textarea
        maxLength={2000}
        rows={6}
        data-error={error != null ? true : undefined}
        {...rest}
        ref={textAreaRef}
        value={value}
        readOnly={readOnly}
        className={cxTextAreaClassName}
        onChange={handleChange}
      />
    </span>
  )
})
