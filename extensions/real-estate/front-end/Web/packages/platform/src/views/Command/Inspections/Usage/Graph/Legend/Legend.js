import { Flex } from '@willow/ui'
import { useGraph } from '../GraphContext'
import Name from '../Name/Name'

export default function Legend() {
  const graph = useGraph()

  return (
    <Flex horizontal fill="wrap" padding="0 0 extraLarge 0" size="medium">
      {graph.names.map((name) => (
        <Name key={name} name={name} />
      ))}
    </Flex>
  )
}
