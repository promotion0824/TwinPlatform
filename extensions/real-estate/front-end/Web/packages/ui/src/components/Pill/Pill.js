import cx from 'classnames'
import styles from './Pill.css'

export default function Pill({ color, className, children, ...rest }) {
  const cxClassName = cx(
    styles.pill,
    {
      [styles.colorGreen]: color === 'green',
      [styles.colorBlue]: color === 'blue',
      [styles.colorYellow]: color === 'yellow',
      [styles.colorOrange]: color === 'orange',
      [styles.colorRed]: color === 'red',
      [styles.colorPanel]: color === 'panel',
    },
    className
  )

  return (
    <span {...rest} className={cxClassName}>
      {children}
    </span>
  )
}
