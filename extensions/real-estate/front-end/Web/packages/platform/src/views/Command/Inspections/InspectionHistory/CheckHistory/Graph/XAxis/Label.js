import { Text, Time } from '@willow/ui'
import styles from './Label.css'

export default function Label({ label }) {
  return (
    <div className={styles.label} style={{ left: label.x }}>
      <Text size="tiny">
        <div>
          <Time value={label.value} format="date" />
        </div>
        <div>
          <Time value={label.value} format="time" />
        </div>
      </Text>
    </div>
  )
}
