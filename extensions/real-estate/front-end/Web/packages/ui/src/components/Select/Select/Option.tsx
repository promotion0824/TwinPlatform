import { ReactNode } from 'react'
import cx from 'classnames'
import { passToFunction } from '../../../utils'
import Button from '../../Button/Button'
import Flex from '../../Flex/Flex'
import Icon from '../../Icon/Icon'
import Text from '../../Text/Text'
import { useDropdown } from '../../Dropdown/DropdownContext'
import { useSelect } from './SelectContext'
import styles from './Option.css'

export default function Option<T>({
  value = undefined,
  selectedValue = undefined,
  type = undefined,
  disabled = false,
  className = undefined,
  children = undefined,
  onClick = undefined,
  iconHidden = false,
  role,
  ...rest
}: {
  value?: T
  selectedValue?: T
  type?: string
  role?: string
  disabled?: boolean
  className?: string
  children?: ReactNode
  onClick?: (e: MouseEvent) => void
  iconHidden?: boolean
}) {
  const dropdown = useDropdown()
  const select = useSelect()

  const nextDisabled = disabled ?? type === 'header'

  const nextValue = (value === undefined ? children : value) ?? null
  const isSelected = select.isSelected(selectedValue ?? nextValue)

  const cxClassName = cx(
    styles.option,
    {
      [styles.selected]: isSelected,
      [styles.typeHeader]: type === 'header',
    },
    passToFunction(className, { isSelected }),
    'ignore-onclickoutside'
  )

  function handleClick(e) {
    select.select(nextValue)
    if (!select.isMultiSelect) {
      dropdown.close()
      dropdown.dropdownRef.current.focus()
    }

    onClick?.(e)
  }

  return (
    <Button
      selected={isSelected}
      role={role}
      {...rest}
      disabled={nextDisabled}
      className={cxClassName}
      onClick={handleClick}
    >
      <Flex
        horizontal
        fill="header"
        align="middle"
        size="medium"
        className={styles.content}
      >
        <Text whiteSpace="nowrap">{children}</Text>
        {!select.unselectable && isSelected && !iconHidden && (
          <Icon icon="cross" className={styles.cross} />
        )}
      </Flex>
    </Button>
  )
}
