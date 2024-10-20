import { forwardRef, useState } from 'react'
import cx from 'classnames'
import { useDebounce } from 'hooks'
import { useFormControl, withFormControl } from 'components/Form/Form'
import Icon from 'components/Icon/Icon'
import styles from './Input.css'

export default withFormControl('')(
  forwardRef((props, forwardedRef) => {
    const {
      value,
      suffix,
      vertical,
      align,
      icon,
      color,
      debounce,
      changeOnBlur = false,
      inputRef,
      className,
      labelClassName,
      iconClassName,
      inputClassName,
      suffixClassName,
      onChange = () => {},
      onFocus = () => {},
      onBlur = () => {},
      ...rest
    } = props

    const control = useFormControl()

    const debouncedOnChange = useDebounce(
      onChange,
      debounce === true ? 300 : debounce
    )

    const [state, setState] = useState({
      value,
      hasFocus: false,
    })

    const derivedValue = state.hasFocus ? state.value : value

    function handleChange(e) {
      const nextValue = e.currentTarget.value

      setState((prevState) => ({
        ...prevState,
        value: nextValue,
      }))

      if (!debounce && !changeOnBlur) {
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

      onFocus(e)
    }

    function handleBlur(e) {
      setState((prevState) => ({
        ...prevState,
        hasFocus: false,
      }))

      if (debounce || changeOnBlur) {
        debouncedOnChange.cancel()

        if (e.currentTarget.value !== value) {
          onChange(e.currentTarget.value)
        }
      }

      onBlur(e)
    }

    const cxClassName = cx(
      styles.input,
      {
        [styles.hasValue]: value != null && value !== '',
        [styles.hasError]: control.error != null,
        [styles.horizontal]: !vertical,
        [styles.vertical]: vertical,
        [styles.right]: align === 'right',
        [styles.disabled]: control.disabled,
        [styles.readOnly]: control.readOnly,
        [styles.dark]: color === 'dark',
      },
      className
    )

    const cxLabelClassName = cx(styles.label, labelClassName)
    const cxIconClassName = cx(styles.icon, iconClassName)
    const cxInputClassName = cx(styles.inputControl, inputClassName)
    const cxSuffixClassName = cx(styles.suffix, suffixClassName)

    return (
      <label // eslint-disable-line
        ref={forwardedRef}
        className={cxClassName}
        data-tooltip={control.title}
      >
        {control.label != null && (
          <span className={cxLabelClassName}>{control.label}</span>
        )}
        {icon && <Icon icon={icon} className={cxIconClassName} />}
        <input
          type="text"
          {...rest}
          ref={inputRef}
          value={derivedValue}
          disabled={control.disabled}
          readOnly={control.readOnly}
          className={cxInputClassName}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
        />
        {suffix != null && <span className={cxSuffixClassName}>{suffix}</span>}
      </label>
    )
  })
)
