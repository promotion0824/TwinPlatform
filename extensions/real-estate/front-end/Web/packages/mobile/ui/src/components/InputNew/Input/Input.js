import { forwardRef, useEffect, useRef, useState } from 'react'
import _ from 'lodash'
import { useUserAgent } from 'providers'
import { useDebounce, useTimer } from 'hooks'
import Dropdown, { DropdownContent } from 'components/DropdownNew/Dropdown'
import Fetch from 'components/Fetch/Fetch'
import { useEffectOnceMounted } from '@willow/common'
import { InputContext } from './InputContext'
import Input from './Input/Input'
import KeyboardHandler from './KeyboardHandler'
import styles from './Input.css'

export default forwardRef(function InputComponent(
  {
    disabled,
    readOnly,
    debounce,
    closeOnEmpty = true,
    blurOnEnter = true,
    url,
    params,
    cache,
    mock,
    notFound,
    content,
    value,
    children,
    onChange,
    onOptionSelect,
    onFocus,
    onBlur,
    onPointerDown,
    onKeyDown,
    controlled,
    ...rest
  },
  forwardedRef
) {
  const userAgent = useUserAgent()
  const ref = useRef()
  const timer = useTimer()
  const inputRef = forwardedRef ?? ref

  const [state, setState] = useState({
    value,
    debouncedValue: value,
    hasFocus: false,
    isOpen: false,
  })
  const debouncedOnChange = useDebounce(
    onChange,
    debounce === true ? 300 : debounce
  )
  const formattedValue = state.value ?? ''
  const formattedDebouncedValue = state.debouncedValue ?? ''
  const derivedUrl = _.isFunction(url) ? url(formattedDebouncedValue) : url
  const derivedParams = _.isFunction(params)
    ? params(formattedDebouncedValue)
    : params

  useEffectOnceMounted(() => {
    const nextValue = state.value

    if (url != null) {
      timer.setTimeout(() => {
        setState((prevState) => ({
          ...prevState,
          debouncedValue: nextValue,
        }))
      }, 500)
    }
  }, [state.value])

  useEffect(() => {
    if (!state.hasFocus || controlled) {
      setState((prevState) => ({
        ...prevState,
        value,
      }))
    }
  }, [value])

  function handleChange(nextValue) {
    setState((prevState) => ({
      ...prevState,
      ...(!controlled ? { value: nextValue } : {}),
      hasFocus: true,
      isOpen: !closeOnEmpty || nextValue !== '',
    }))

    if (debounce) {
      debouncedOnChange?.(nextValue)
    } else {
      onChange?.(nextValue)
    }
  }

  function handleFocus(e) {
    setState((prevState) => ({
      ...prevState,
      value,
      hasFocus: true,
      isOpen: true,
    }))

    onFocus?.(e)
  }

  function handleBlur(e) {
    setState((prevState) => ({
      ...prevState,
      value,
      hasFocus: false,
      isOpen: false,
    }))

    if (debounce) {
      debouncedOnChange.cancel()

      if (e.currentTarget.value !== value) {
        onChange?.(e.currentTarget.value)
      }
    }

    onBlur?.(e)
  }

  function handlePointerDown(e) {
    const LEFT_BUTTON = 0
    if (!disabled && !readOnly && e.button === LEFT_BUTTON) {
      setState((prevState) => ({
        ...prevState,
        hasFocus: true,
        isOpen: true,
      }))
    }

    onPointerDown?.(e)
  }

  function handleKeyDown(e) {
    if (userAgent.isIpad && e.key === 'Enter' && blurOnEnter) {
      inputRef.current.blur()
    }

    onKeyDown?.(e)
  }

  function setIsOpen(isOpen) {
    setState((prevState) => ({
      ...prevState,
      isOpen,
    }))
  }

  const context = {
    value: formattedValue,

    select(nextValue) {
      setState((prevState) => ({
        ...prevState,
        isOpen: false,
        hasFocus: false,
      }))

      onOptionSelect?.(nextValue)
    },
  }

  const isOpen =
    state.isOpen &&
    !disabled &&
    !readOnly &&
    children != null &&
    derivedUrl != null

  return (
    <InputContext.Provider value={context}>
      <Dropdown dropdownRef={inputRef} isOpen={isOpen} onChange={setIsOpen}>
        <Input
          {...rest}
          ref={inputRef}
          disabled={disabled}
          readOnly={readOnly}
          value={formattedValue}
          onChange={handleChange}
          onFocus={handleFocus}
          onBlur={handleBlur}
          onPointerDown={handlePointerDown}
          onKeyDown={handleKeyDown}
        >
          {content}
        </Input>
        <DropdownContent useMinWidth contentClassName={styles.content}>
          <Fetch
            url={derivedUrl}
            params={derivedParams}
            cache={cache}
            mock={mock}
            notFound={notFound}
          >
            {children}
          </Fetch>
          <KeyboardHandler />
        </DropdownContent>
      </Dropdown>
    </InputContext.Provider>
  )
})
