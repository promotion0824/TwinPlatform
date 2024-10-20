import cx from 'classnames'
import Flex from 'components/Flex/Flex'
import styles from './ProgressTotal.css'

export default function ProgressTotal({ value, total, color = 'green' }) {
  const percent = (value / total) * 100

  const cxClassName = cx({
    [styles.colorGreen]: color === 'green',
    [styles.colorRed]: color === 'red',
  })

  return (
    <Flex horizontal align="middle" size="medium" className={cxClassName}>
      <svg viewBox="0 0 36 36" className={styles.svg}>
        <path
          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          fill="none"
          strokeWidth="2.5"
          strokeDasharray="100, 100"
        />
        <path
          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          fill="none"
          strokeWidth="2.5"
          strokeDasharray={`${percent}, 100`}
        />
      </svg>
      <span>
        {value}/{total}
      </span>
    </Flex>
  )
}
