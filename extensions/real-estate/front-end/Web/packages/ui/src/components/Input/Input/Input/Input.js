import { forwardRef, useLayoutEffect, useState } from 'react'
import { useForwardedRef } from '@willow/ui'
import cx from 'classnames'
import Icon from 'components/Icon/Icon'
import styles from './Input.css'

export default forwardRef(function Input(
  {
    value,
    error,
    icon,
    readOnly,
    disabled,
    placeholder,
    preservePlaceholder = false,
    height,
    width,
    border = 'top left bottom right',
    className,
    iconClassName,
    inputClassName,
    onChange,
    ...rest
  },
  forwardedRef
) {
  const [hasValue, setHasValue] = useState(false)

  const inputRef = useForwardedRef(forwardedRef)

  useLayoutEffect(() => {
    const nextHasValue = inputRef.current?.value?.length > 0

    if (hasValue !== nextHasValue) {
      setHasValue(nextHasValue)
    }
  }, [value])

  const modifiedPlaceholder = preservePlaceholder
    ? placeholder
    : `- ${placeholder} -`
  const nextPlaceholder = placeholder != null ? modifiedPlaceholder : undefined

  const cxClassName = cx(
    styles.input,
    {
      [styles.readOnly]: readOnly,
      [styles.disabled]: disabled,
      [styles.hasValue]: hasValue,
      [styles.hasError]: error != null,
      [styles.hasIcon]: icon != null,
      [styles.heightMedium]: height === 'medium',
      [styles.heightLarge]: height === 'large',
      [styles.widthTiny]: width === 'tiny',
      [styles.borderTop]: border?.split(' ').includes('top'),
      [styles.borderLeft]: border?.split(' ').includes('left'),
      [styles.borderBottom]: border?.split(' ').includes('bottom'),
      [styles.borderRight]: border?.split(' ').includes('right'),
    },
    className
  )
  const cxIconClassName = cx(styles.icon, iconClassName)
  const cxInputClassName = cx(styles.inputControl, inputClassName)

  function handleChange(e) {
    const nextHasValue = e.currentTarget.value.length > 0
    if (hasValue !== nextHasValue) {
      setHasValue(nextHasValue)
    }

    onChange(e.currentTarget.value)
  }

  return (
    <span className={cxClassName}>
      {typeof icon === 'string' ? (
        <Icon icon={icon} className={cxIconClassName} />
      ) : (
        icon
      )}
      <input
        ref={inputRef}
        type="text"
        autoComplete="off"
        maxLength={300}
        spellCheck={false}
        data-error={error != null ? true : undefined}
        {...rest}
        value={value}
        readOnly={readOnly}
        disabled={disabled}
        placeholder={nextPlaceholder}
        className={cxInputClassName}
        onChange={handleChange}
      />
    </span>
  )
})
