import cx from 'classnames'
import { Time } from '@willow/ui'
import styles from './CheckEntry.css'

export default function CheckEntry({ check, entry, type, typeValue }) {
  if (type === 'Date') {
    const time =
      check.statistics.workableCheckStatus === 'completed'
        ? check.lastSubmittedRecord?.dateValue
        : undefined

    return (
      <div className={styles.wrapper}>
        <div className={styles.item}>
          {time != null && <Time value={time} format="date" />}
          {time == null && <span>-</span>}
        </div>
      </div>
    )
  }

  if (type === 'List') {
    return (
      <div className={styles.wrapper}>
        {typeValue.split('|').map((item, i) => (
          <div
            key={i} // eslint-disable-line
            className={cx(styles.item, { [styles.active]: item === entry })}
          >
            {item}
          </div>
        ))}
      </div>
    )
  }

  return (
    <div className={styles.wrapper}>
      <div className={cx(styles.item, styles.active)}>{entry ?? '-'}</div>
      <div className={styles.item}>{typeValue}</div>
    </div>
  )
}
