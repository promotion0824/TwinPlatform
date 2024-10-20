import Portal from 'components/Portal/Portal'
import Content from './Content'
import { useDropdown } from './DropdownContext'

export default function DropdownContent({ children, ...rest }) {
  const dropdown = useDropdown()

  if (!dropdown.isOpen) {
    return null
  }

  return (
    <Portal>
      <Content {...rest}>{children}</Content>
    </Portal>
  )
}
