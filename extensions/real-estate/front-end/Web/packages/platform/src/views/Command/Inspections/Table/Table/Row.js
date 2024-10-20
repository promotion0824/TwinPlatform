import { useHistory } from 'react-router'
import cx from 'classnames'
import styles from './Table.css'

export default function Row({
  to,
  selected,
  selectedType,
  isVisible = true,
  color,
  className,
  style,
  children,
  onClick,
  ...rest
}) {
  const history = useHistory()

  const isClickable = to != null || onClick != null

  const cxClassName = cx(
    styles.row,
    {
      [styles.isHidden]: !isVisible,
      [styles.isClickable]: isClickable,
      [styles.isSelected]: selected,
      [styles.selectedTypeBasic]: selectedType === 'basic',
      [styles.hasColor]: color,
    },
    className
  )

  const nextStyle =
    color != null
      ? {
          '--row-color': color,
          ...style,
        }
      : style

  function handleClick(e) {
    if (to != null) {
      history.push(to)
    }

    onClick?.(e)
  }

  return (
    <tr
      {...rest}
      className={cxClassName}
      onClick={handleClick}
      style={nextStyle}
    >
      {children}
    </tr>
  )
}
