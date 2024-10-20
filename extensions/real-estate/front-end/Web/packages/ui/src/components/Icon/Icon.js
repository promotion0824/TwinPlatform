import _ from 'lodash'
import cx from 'classnames'
import * as icons from './icons'
import styles from './Icon.css'

export default function Icon({
  icon,
  color = undefined,
  size = 'medium',
  className = undefined,
  style = undefined,
  ...rest
}) {
  let nextColor = color
  if (color == null) {
    if (icon === 'error') {
      nextColor = 'red'
    }
    if (icon === 'warning') {
      nextColor = 'yellow'
    }
    if (icon === 'ok') {
      nextColor = 'green'
    }
  }

  const IconComponent = icons[_.camelCase(icon)] ?? icons.defaultIcon
  const cxClassName = cx(
    styles.icon,
    {
      [styles.purple]: nextColor === 'purple',
      [styles.red]: nextColor === 'red',
      [styles.redGraph]: nextColor === 'redGraph',
      [styles.orange]: nextColor === 'orange',
      [styles.yellow]: nextColor === 'yellow',
      [styles.yellowGraph]: nextColor === 'yellowGraph',
      [styles.green]: nextColor === 'green',
      [styles.greenGraph]: nextColor === 'greenGraph',
      [styles.white]: nextColor === 'white',
      [styles.dark]: nextColor === 'dark',
      [styles.darkGraph]: nextColor === 'darkGraph',
      [styles.sizeExtraTiny]: size === 'extraTiny',
      [styles.sizeTiny]: size === 'tiny',
      [styles.sizeSmall]: size === 'small',
      [styles.sizeMedium]: size === 'medium',
      [styles.sizeLarge]: size === 'large',
      // [styles.sizeXL]: size === "xl", // currently unused
      [styles.sizeXXL]: size === 'xxl',
      [styles.progress]: icon === 'progress',
    },
    className
  )

  return <IconComponent {...rest} style={style} className={cxClassName} />
}
