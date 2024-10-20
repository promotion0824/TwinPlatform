import cx from 'classnames'
import { styled } from 'twin.macro'
import { passToFunction } from '@willow/ui'
import Button from 'components/Button/Button'
import Flex from 'components/Flex/Flex'
import Text from 'components/Text/Text'
import Icon from 'components/Icon/Icon'
import { useDropdown } from './DropdownContext'
import styles from './DropdownHeaderButton.css'

export default function DropdownHeaderButton({
  customHeader = false,
  readOnly,
  disabled,
  className,
  iconClassName,
  headerContentClassName,
  children,
  onPointerDown,
  onKeyDown,
  ...rest
}) {
  const dropdown = useDropdown()

  const buttonClassName = cx(
    styles.dropdownHeaderButton,
    {
      [styles.open]: dropdown.isOpen,
      [styles.readOnly]: readOnly,
      [styles.disabled]: disabled,
    },
    passToFunction(className, dropdown)
  )
  function handlePointerDown(e) {
    e.stopPropagation()

    const LEFT_BUTTON = 0
    if (e.button === LEFT_BUTTON) {
      dropdown.toggle()
    }

    onPointerDown?.(e)
  }

  function handleKeyDown(e) {
    if (e.key === 'Enter') {
      dropdown.toggle()
    }
    if (e.key === 'ArrowDown') {
      dropdown.open()
    }

    onKeyDown?.(e)
  }

  return (
    <Button
      {...rest}
      ref={dropdown.dropdownRef}
      error={null}
      readOnly={readOnly}
      disabled={disabled}
      className={buttonClassName}
      iconClassName={cx(styles.icon, iconClassName)}
      onPointerDown={handlePointerDown}
      onKeyDown={handleKeyDown}
    >
      {!customHeader && (
        <Inner className={cx(styles.content, headerContentClassName)}>
          {children != null && (
            <Text tw="flex-1" whiteSpace="nowrap">
              {passToFunction(children, dropdown)}
            </Text>
          )}
          <Chevron icon="chevron" className={styles.chevron} />
        </Inner>
      )}
      {customHeader && passToFunction(children, dropdown)}
    </Button>
  )
}

const Inner = styled.div({
  display: 'flex',
  alignItems: 'center',
  width: '100%',
  minWidth: 0,
})

const Chevron = styled(Icon)({
  flex: '0 1 auto',
  marginLeft: 'var(--padding)',
})
