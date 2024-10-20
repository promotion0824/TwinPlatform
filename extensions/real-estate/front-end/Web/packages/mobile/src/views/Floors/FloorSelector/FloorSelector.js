import { Select, Spacing, Option } from '@willow/mobile-ui'
import { useFloor } from 'providers'
import styles from './FloorSelector.css'

export default function FloorSelector({ floors }) {
  const floorContext = useFloor()

  function handleClick(option) {
    floorContext.setFloor(floors.find((x) => x.id === option))
  }

  return (
    <Spacing padding="0 medium">
      <Select
        className={styles.floors}
        value={floorContext.floor?.id}
        text={(value) => {
          const floor = floors.find((x) => x.id === value)
          return floor ? floor.code : ''
        }}
        placeholder="Select floor"
        onChange={(nextValue) => handleClick(nextValue)}
      >
        {floors.map((floor) => (
          <Option key={floor.id} value={floor.id}>
            {floor.code}
          </Option>
        ))}
      </Select>
    </Spacing>
  )
}
