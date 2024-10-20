import { useLocation } from 'react-router'
import cx from 'classnames'
import Button from 'components/Button/Button'
import styles from './SubmenuButton.css'

export default function SubmenuButton({
  to,
  exclude,
  selected: selectedProp,
  className,
  children,
  isPurpleBackground = false,
  isGrayBackground = false,
  ...rest
}) {
  const location = useLocation()

  let selected = selectedProp
  if (selected == null) {
    selected = location.pathname === to
    if (exclude != null) {
      selected =
        location.pathname.startsWith(to) &&
        !exclude.some((e) => location.pathname.startsWith(`${to}${e}`))
    }
  }

  const cxClassName = cx(
    styles.submenuButton,
    { [styles.purpleBackground]: isPurpleBackground },
    { [styles.grayBackground]: isGrayBackground },
    className
  )

  return (
    <Button
      color={selected ? 'purple' : 'grey'}
      height="small"
      width="small"
      {...rest}
      to={to}
      selected={selected}
      className={cxClassName}
    >
      {children}
    </Button>
  )
}
