import { forwardRef, useRef, useState } from 'react'
import cx from 'classnames'
import Button from 'components/ButtonNew/Button'
import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import { DropdownContext } from './DropdownContext'
import styles from './Dropdown.css'

export { useDropdown } from './DropdownContext'
export { default as DropdownButton } from './DropdownButton'
export { default as DropdownContent } from './DropdownContent'

export default forwardRef(function Dropdown(
  {
    'data-segment': dataSegment,
    disabled = false,
    readOnly = false,
    isOpen,
    value,
    dropdownIcon = 'chevron',
    dropdownIconSize,
    dropdownRef,
    showIcon = true,
    className,
    dropdownIconClassName,
    children,
    onChange,
    onPointerDown = () => {},
    onKeyDown = () => {},
    ...rest
  },
  forwardedRef
) {
  const ref = useRef()
  const nextDropdownRef = dropdownRef ?? forwardedRef ?? ref
  const contentRef = useRef()

  const [state, setState] = useState({
    isOpen,
  })

  const nextIsOpen = isOpen ?? state.isOpen

  const cxClassName = cx(
    styles.dropdown,
    {
      [styles.isDisabled]: disabled || readOnly,
      [styles.isOpen]: nextIsOpen,
    },
    className
  )
  const cxDropdownIconClassName = cx(styles.icon, dropdownIconClassName)

  function handlePointerDown(e) {
    const LEFT_BUTTON = 0
    if (e.button === LEFT_BUTTON) {
      setState((prevState) => ({
        ...prevState,
        isOpen: true,
      }))

      onPointerDown(e)
    }
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      setState((prevState) => ({
        ...prevState,
        isOpen: true,
      }))
    } else if (e.key === 'ArrowDown') {
      e.preventDefault()

      setState((prevState) => ({
        ...prevState,
        isOpen: true,
      }))
    }

    onKeyDown(e)
  }

  const context = {
    dropdownRef: nextDropdownRef,
    contentRef,
    isOpen: nextIsOpen,

    close() {
      setState((prevState) => ({
        ...prevState,
        isOpen: false,
      }))

      onChange?.(false)
    },
  }

  return (
    <DropdownContext.Provider value={context}>
      {dropdownRef == null && (
        <Button
          {...rest}
          ref={nextDropdownRef}
          preventDefault={false}
          disabled={disabled}
          readOnly={readOnly}
          className={cxClassName}
          onPointerDown={handlePointerDown}
          onKeyDown={handleKeyDown}
          data-segment={dataSegment}
        >
          {showIcon ? (
            <Spacing
              horizontal
              type="header"
              align="middle"
              overflow="hidden"
              className={styles.content}
            >
              <Text>{children}</Text>
              <Icon
                icon={dropdownIcon}
                size={dropdownIconSize}
                className={cxDropdownIconClassName}
              />
            </Spacing>
          ) : (
            children
          )}
        </Button>
      )}
      {dropdownRef != null && children}
    </DropdownContext.Provider>
  )
})
