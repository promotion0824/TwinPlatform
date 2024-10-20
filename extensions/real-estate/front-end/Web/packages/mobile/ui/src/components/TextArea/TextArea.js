import { forwardRef, useState, useRef, useEffect } from 'react'
import cx from 'classnames'
import { useDebounce } from 'hooks'
import { useFormControl, withFormControl } from 'components/Form/Form'
import styles from './TextArea.css'

export default withFormControl('')(
  forwardRef(function TextArea(props, forwardedRef) {
    const {
      value,
      vertical,
      debounce,
      changeOnBlur = false,
      className,
      children,
      onChange = () => {},
      onFocus = () => {},
      onBlur = () => {},
      focusOnError = false,
      ...rest
    } = props
    const control = useFormControl()

    const textAreaRef = useRef()

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
      styles.textArea,
      {
        [styles.hasValue]: value != null && value !== '',
        [styles.hasError]: control.error != null,
        [styles.horizontal]: !vertical,
        [styles.vertical]: vertical,
        [styles.disabled]: control.disabled,
        [styles.readOnly]: control.readOnly,
      },
      className
    )

    useEffect(() => {
      if (focusOnError && control.error != null) {
        textAreaRef.current.focus()
      }
    }, [focusOnError, control])

    return (
      <label // eslint-disable-line
        ref={forwardedRef}
        className={cxClassName}
      >
        {control.label != null && (
          <span className={styles.label}>{control.label}</span>
        )}
        <textarea
          rows={6}
          {...rest}
          value={derivedValue}
          disabled={control.disabled}
          readOnly={control.readOnly}
          data-tooltip={control.title}
          className={styles.control}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          ref={textAreaRef}
        />
      </label>
    )
  })
)
