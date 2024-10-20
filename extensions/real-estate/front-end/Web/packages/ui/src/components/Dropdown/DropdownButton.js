import cx from 'classnames'
import Button from 'components/Button/Button'
import Text from 'components/Text/Text'
import { useDropdown } from './DropdownContext'
import styles from './DropdownButton.css'

export default function DropdownButton({
  closeOnClick = true,
  className,
  children,
  onClick,
  ...rest
}) {
  const dropdown = useDropdown()

  const cxClassName = cx(styles.dropdownButton, className)

  function handleClick(e) {
    if (closeOnClick) {
      dropdown.close()
    }

    onClick?.(e)
  }

  return (
    <Button className={cxClassName} {...rest} onClick={handleClick}>
      <Text whiteSpace="nowrap">{children}</Text>
    </Button>
  )
}
