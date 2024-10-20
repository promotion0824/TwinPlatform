import Portal from 'components/Portal/Portal'
import Content from './Content'
import { useDropdown } from './DropdownContext'

export default function DropdownContent({ ...rest }) {
  const dropdown = useDropdown()

  if (!dropdown.isOpen) {
    return null
  }

  return (
    <Portal>
      <Content {...rest} />
    </Portal>
  )
}
