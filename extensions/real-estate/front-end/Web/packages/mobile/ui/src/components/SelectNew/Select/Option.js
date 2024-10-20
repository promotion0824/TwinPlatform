import cx from 'classnames'
import Icon from 'components/Icon/Icon'
import Spacing from 'components/Spacing/Spacing'
import Text from 'components/Text/Text'
import { useDropdown, DropdownButton } from 'components/DropdownNew/Dropdown'
import { useSelect } from './SelectContext'
import styles from './Option.css'

export default function Option({
  value,
  className = undefined,
  children,
  onClick = undefined,
  ...rest
}) {
  const dropdown = useDropdown()
  const select = useSelect()

  const nextValue = value === undefined ? children : value
  const isSelected = select.isSelected(nextValue)

  const cxClassName = cx(styles.option, className)

  function handleClick() {
    select.select(nextValue)
    dropdown.dropdownRef.current.focus()

    onClick?.(nextValue)
  }

  return (
    <DropdownButton
      {...rest}
      role="option"
      selected={isSelected}
      className={cxClassName}
      onClick={handleClick}
    >
      <Spacing horizontal width="100%" type="header" overflow="hidden">
        <Text>{children}</Text>
        {select.unselectable && isSelected && (
          <Icon icon="close" size="small" />
        )}
      </Spacing>
    </DropdownButton>
  )
}
