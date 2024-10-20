import cx from 'classnames'
import numberUtils from 'utils/numberUtils'
import styles from './Number.css'

export default function Number(props) {
  const { value, format, className, ...rest } = props

  const formattedValue = numberUtils.format(value, format)

  return (
    <span {...rest} className={cx(styles.number, className)}>
      {formattedValue}
    </span>
  )
}
