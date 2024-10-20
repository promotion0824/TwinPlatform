import cx from 'classnames'
import Button from 'components/ButtonNew/Button'
import { useDropdown } from './DropdownContext'
import styles from './DropdownButton.css'

export default function DropdownButton({
  selected,
  closeOnClick = true,
  className,
  children,
  onClick = () => {},
  ...rest
}) {
  const dropdown = useDropdown()

  const cxClassName = cx(
    styles.dropdownButton,
    {
      [styles.isSelected]: selected,
    },
    className
  )

  function handleClick(e) {
    if (closeOnClick) {
      dropdown.close()
    }

    onClick(e)
  }

  return (
    <Button
      {...rest}
      className={cxClassName}
      selected={selected}
      onClick={handleClick}
    >
      {children}
    </Button>
  )
}
