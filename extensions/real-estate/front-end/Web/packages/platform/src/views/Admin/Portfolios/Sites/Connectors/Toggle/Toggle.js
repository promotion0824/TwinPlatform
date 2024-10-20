import { Checkbox, Text } from '@willow/ui'
import cx from 'classnames'
import styles from './Toggle.css'

export default function Toggle({
  onLabel,
  offLabel,
  value,
  name,
  onChange,
  ...rest
}) {
  const cxSliderRound = cx(styles.slider, styles.round, {
    [styles.isChecked]: value,
  })
  const cxCheckbox = cx(styles.checkbox, {
    [styles.isChecked]: value,
  })
  return (
    /* eslint-disable jsx-a11y/label-has-associated-control */
    <label className={styles.switch} {...rest}>
      <Checkbox
        className={cxCheckbox}
        name={name}
        onChange={onChange}
        value={value}
      />
      <div className={cxSliderRound}>
        <Text size="small">
          <span className={styles.on}>{onLabel}</span>
          <span className={styles.off}>{offLabel}</span>
        </Text>
      </div>
    </label>
  )
}
