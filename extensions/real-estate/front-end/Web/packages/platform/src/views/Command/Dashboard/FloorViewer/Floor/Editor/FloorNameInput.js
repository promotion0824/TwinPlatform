import { useFloor } from '../FloorContext'
import styles from './FloorNameInput.css'

export default function FloorNameInput() {
  const floor = useFloor()

  function handleChange(e) {
    floor.updateName(e.currentTarget.value)
  }

  if (floor.isReadOnly) {
    return floor.floorName
  }

  return (
    <input
      type="text"
      value={floor.floorName}
      className={styles.input}
      onChange={handleChange}
    />
  )
}
