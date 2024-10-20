import cx from 'classnames'
import * as icons from './icons'
import styles from './Icon.css'

export default function Icon({
  icon,
  size = 'medium',
  color = undefined,
  className = undefined,
  ...rest
}) {
  const IconComponent = icons[icon] ?? icons.defaultIcon

  const cxClassName = cx(
    styles.icon,
    {
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.colorGreen]: color === 'green',
      [styles.colorGreenDark]: color === 'greenDark',
      [styles.colorYellow]: color === 'yellow',
      [styles.colorYellowDark]: color === 'yellowDark',
      [styles.colorOrange]: color === 'orange',
      [styles.colorOrangeDark]: color === 'orangeDark',
      [styles.colorRed]: color === 'red',
      [styles.colorRedDark]: color === 'redDark',
      [styles.progress]: icon === 'progress',
    },
    className
  )

  return <IconComponent {...rest} className={cxClassName} />
}
