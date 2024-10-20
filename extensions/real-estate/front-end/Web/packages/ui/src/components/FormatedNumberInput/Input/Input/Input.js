import { forwardRef } from 'react'
import cx from 'classnames'
import { NumericFormat } from 'react-number-format'
import Icon from 'components/Icon/Icon'
import styles from './Input.css'

export default forwardRef(function Input(
  {
    value,
    disabled,
    error,
    readOnly,
    width,
    icon,
    inputType,
    children,
    className,
    iconClassName,
    inputClassName,
    onChange,
    ...rest
  },
  forwardedRef
) {
  const cxClassName = cx(
    styles.input,
    {
      [styles.hasValue]: value.length > 0,
      [styles.isDisabled]: disabled || readOnly,
      [styles.widthSmall]: width === 'small',
      [styles.hasError]: error != null,
      [styles.typeSearch]: inputType === 'search',
      [styles.hasIcon]: icon != null,
      [styles.okIcon]: icon === 'ok',
    },
    className
  )
  const cxIconClassName = cx(styles.icon, iconClassName)
  const cxInputClassName = cx(styles.inputControl, inputClassName)

  return (
    <span className={cxClassName}>
      {icon && <Icon icon={icon} className={cxIconClassName} />}
      <NumericFormat
        type="text"
        data-error={error != null ? true : undefined}
        {...rest}
        getInputRef={forwardedRef}
        disabled={disabled}
        readOnly={readOnly}
        className={cxInputClassName}
        value={value}
        onValueChange={(e) => {
          onChange?.(e.value)
        }}
      />
      {children}
    </span>
  )
})
