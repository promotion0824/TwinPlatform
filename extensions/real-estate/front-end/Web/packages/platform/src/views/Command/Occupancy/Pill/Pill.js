import { Button } from '@willow/ui'
import cx from 'classnames'
import styles from './Pill.css'

export default function PillComponent({
  color,
  href,
  to,
  selected,
  hovering,
  padding,
  className,
  children,
  onClick,
  ...rest
}) {
  const isClickable = href != null || to != null || onClick != null

  const cxClassName = cx(
    styles.pill,
    {
      [styles.noPadding]: padding === '0',
      [styles.isClickable]: isClickable,
      [styles.isSelected]: selected,
      [styles.isHovering]: hovering,
      [styles.colorGreen]: color === 'green',
      [styles.colorRed]: color === 'red',
      [styles.colorOrange]: color === 'orange',
      [styles.colorYellow]: color === 'yellow',
      [styles.colorSelectedText]: color === 'selectedText',
    },
    className
  )

  return isClickable ? (
    <Button
      ripple
      {...rest}
      href={href}
      to={to}
      className={cxClassName}
      onClick={onClick}
    >
      {children}
    </Button>
  ) : (
    <span {...rest} className={cxClassName}>
      {children}
    </span>
  )
}
