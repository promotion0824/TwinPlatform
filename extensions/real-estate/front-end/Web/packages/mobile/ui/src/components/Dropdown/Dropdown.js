import { useRef, useState } from 'react'
import cx from 'classnames'
import Button from 'components/Button/Button'
import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import { DropdownContext } from './DropdownContext'
import styles from './Dropdown.css'

export { useDropdown } from './DropdownContext'
export { default as DropdownButton } from './DropdownButton'
export { default as DropdownContent } from './DropdownContent'

export default function Dropdown({
  type,
  disabled = false,
  readOnly = false,
  className,
  children,
  onPointerDown = () => {},
  onKeyDown = () => {},
  showBorder,
  iconClassName,
  ...rest
}) {
  const dropdownRef = useRef()
  const containerRef = useRef()
  const contentRef = useRef()
  const [isOpen, setIsOpen] = useState(false)

  const cxClassName = cx(
    styles.dropdown,
    {
      [styles.isDisabled]: disabled || readOnly,
      [styles.isOpen]: isOpen,
    },
    className
  )

  function handlePointerDown(e) {
    const LEFT_BUTTON = 0
    if (e.button === LEFT_BUTTON) {
      setIsOpen(true)

      onPointerDown(e)
    }
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      setIsOpen(true)
    }

    onKeyDown(e)
  }

  const context = {
    dropdownRef,
    containerRef,
    contentRef,
    isOpen,

    close() {
      setIsOpen(false)
    },
  }

  return (
    <DropdownContext.Provider value={context}>
      <Button
        {...rest}
        ref={dropdownRef}
        readOnly={readOnly}
        className={cxClassName}
        onPointerDown={handlePointerDown}
        onKeyDown={handleKeyDown}
      >
        <Spacing
          horizontal
          type="header"
          align="middle"
          overflow="hidden"
          className={styles.content}
        >
          <Text>{children}</Text>
          {type !== 'none' && (
            <Icon icon="chevron" className={cx(styles.icon, iconClassName)} />
          )}
        </Spacing>
      </Button>
    </DropdownContext.Provider>
  )
}
