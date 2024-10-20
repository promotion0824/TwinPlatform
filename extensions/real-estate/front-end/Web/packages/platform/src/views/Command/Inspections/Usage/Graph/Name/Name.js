import { Flex } from '@willow/ui'
import { useGraph } from '../GraphContext'
import styles from './Name.css'

export default function Name({ name }) {
  const graph = useGraph()

  return (
    <Flex horizontal align="middle" size="small">
      <div
        className={styles.name}
        style={{
          backgroundColor: graph.getColor(name),
        }}
      />
      <span>{name}</span>
    </Flex>
  )
}
