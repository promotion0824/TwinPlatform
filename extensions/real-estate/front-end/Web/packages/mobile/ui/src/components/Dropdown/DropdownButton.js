import cx from 'classnames'
import Button from 'components/Button/Button'
import { useDropdown } from './DropdownContext'
import styles from './DropdownButton.css'

export default function DropdownButton({
  selected,
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
    dropdown.close()

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
