/* eslint-disable complexity */
import { forwardRef, useEffect, useRef, useState, Fragment } from 'react'
import _ from 'lodash'
import cx from 'classnames'
import { passToFunction, useForwardedRef, useTimer } from '@willow/ui'

import Fetch from 'components/Fetch/Fetch'
import Input from 'components/Input/Input/Input/Input'
import Portal from 'components/Portal/Portal'
import TypeaheadContent from './TypeaheadContent'
import { TypeaheadContext } from './TypeaheadContext'
import styles from './Typeahead.css'

export default forwardRef(function Typeahead(
  {
    type = 'select',
    value,
    selected,
    readOnly,
    disabled,
    icon = 'search',
    url,
    params,
    cache,
    notFound,
    children,
    onChange,
    onSelect,
    onBlur,
    /**
     * When noFetch is true, Typeahead will not fetch data that will be used
     * for the dropdown options. Instead, the children props passed will be
     * used for the dropdown options.
     */
    noFetch = false,
    className,
    zIndex,
    ...rest
  },
  forwardedRef
) {
  const timer = useTimer()

  const [isOpen, setIsOpen] = useState(false)
  const [debouncedValue, setDebouncedValue] = useState(value)

  const inputRef = useForwardedRef(forwardedRef)
  const contentRef = useRef()

  useEffect(() => {
    async function update() {
      if (value === '') {
        timer.clearTimeout()
        setDebouncedValue(value)
        return
      }

      await timer.setTimeout(300)

      setDebouncedValue(value)
    }

    update()
  }, [value])

  const context = {
    inputRef,
    contentRef,

    selected,

    onBlur() {
      setIsOpen(false)

      onBlur?.()
    },

    select(nextValue) {
      inputRef.current.focus()
      setIsOpen(false)

      onSelect(nextValue)
    },

    close() {
      setIsOpen(false)
    },

    isSelected(nextValue) {
      return _.isEqual(value, nextValue)
    },
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      if (isOpen) {
        e.preventDefault()
        setIsOpen(false)
      }

      return
    }

    if (
      e.key !== 'ArrowLeft' &&
      e.key !== 'ArrowRight' &&
      e.key !== 'Home' &&
      e.key !== 'End'
    ) {
      setIsOpen(true)
    }

    if (e.key === 'Tab') {
      context.onBlur()
    }
  }

  let nextIcon
  if (selected != null) {
    nextIcon = selected ? 'ok' : icon
  }

  const cxClassName = cx(
    styles.input,
    {
      [styles.selected]: selected,
      [styles.open]: isOpen,
    },
    className
  )

  const Content = type === 'select' ? TypeaheadContent : Fragment

  return (
    <TypeaheadContext.Provider value={context}>
      <Input
        {...rest}
        ref={inputRef}
        icon={nextIcon}
        className={cxClassName}
        iconClassName={styles.icon}
        value={value}
        readOnly={readOnly}
        disabled={disabled}
        onChange={onChange}
        onKeyDown={handleKeyDown}
        onClick={() => setIsOpen(true)}
        onFocus={() => setIsOpen(true)}
        onBlur={() => {
          if (!isOpen || (type === 'select' && value === '')) {
            context.onBlur()
          }
        }}
      />
      {isOpen &&
        (type !== 'select' || debouncedValue.length > 0) &&
        !readOnly &&
        !disabled &&
        !noFetch && (
          <Portal>
            <Content zIndex={zIndex}>
              <Fetch
                url={passToFunction(url, debouncedValue)}
                params={passToFunction(params, debouncedValue)}
                cache={cache}
                notFound={notFound}
                progress={type === 'text' ? null : undefined}
              >
                {(response) =>
                  response?.length > 0 && passToFunction(children, response)
                }
              </Fetch>
            </Content>
          </Portal>
        )}
      {isOpen && noFetch && value.length !== 0 && (
        <Portal>
          <Content zIndex={zIndex}>{children}</Content>
        </Portal>
      )}
    </TypeaheadContext.Provider>
  )
})
