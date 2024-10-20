import cx from 'classnames'
import numberUtils from 'utils/numberUtils'
import styles from './Number.css'

export default function Number({
  value,
  format = ',',
  invalid = '',
  className = undefined,
  ...rest
}) {
  const formattedValue = numberUtils.format(value, format)

  return (
    <span {...rest} className={cx(styles.number, className)}>
      {formattedValue !== '' ? formattedValue : invalid}
    </span>
  )
}
