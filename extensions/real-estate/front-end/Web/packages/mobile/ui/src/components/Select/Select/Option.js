import Text from 'components/Text/Text'
import { useDropdown, DropdownButton } from 'components/Dropdown/Dropdown'
import { useSelect } from './SelectContext'

export default function Option({
  value,
  selected,
  children,
  onClick,
  ...rest
}) {
  const dropdown = useDropdown()
  const select = useSelect()

  const nextValue = value ?? children
  const isSelected = selected ?? select.isSelected(nextValue)

  function handleClick() {
    select.select(nextValue)
    dropdown.dropdownRef.current.focus()

    onClick?.(nextValue)
  }

  return (
    <DropdownButton {...rest} selected={isSelected} onClick={handleClick}>
      <Text>{children}</Text>
    </DropdownButton>
  )
}
