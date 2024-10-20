import { Spacing } from '@willow/mobile-ui'
import styles from './Progress.css'

export default function Progress({ min, max }) {
  if (max === 0) {
    return null
  }

  const value = (min / max) * 100

  return (
    <Spacing horizontal align="middle" size="medium">
      <svg viewBox="0 0 36 36" className={styles.svg}>
        <path
          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          fill="none"
          strokeWidth="3"
          strokeDasharray="100, 100"
        />
        <path
          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831"
          fill="none"
          strokeWidth="3"
          strokeDasharray={`${value}, 100`}
        />
      </svg>
      <span>
        {min}/{max}
      </span>
    </Spacing>
  )
}
