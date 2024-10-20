import cx from 'classnames'
import * as icons from './icons'
import styles from './Icon.css'

export default function Icon({
  icon,
  color,
  size = 'medium',
  className,
  ...rest
}) {
  let nextColor = color
  if (color == null) {
    if (icon === 'error') {
      nextColor = 'red'
    }
  }

  const IconComponent = icons[icon] ?? icons.defaultIcon
  const cxClassName = cx(
    styles.icon,
    {
      [styles.blue]: nextColor === 'blue',
      [styles.red]: nextColor === 'red',
      [styles.white]: nextColor === 'white',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      [styles.progress]: icon === 'progress',
    },
    className
  )

  return <IconComponent role="img" {...rest} className={cxClassName} />
}
