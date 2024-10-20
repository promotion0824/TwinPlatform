import { useEffect, useState } from 'react'
import { useSize, Number, Text } from '@willow/ui'
import { useGraph } from '../../../GraphContext'
import styles from './YAxis.css'

export default function YAxis({ graphRef, graph }) {
  const graphContext = useGraph()
  const size = useSize(graphRef)

  const [labels, setLabels] = useState([])

  function refresh() {
    const length = Math.max(2, 1 + 2 * Math.floor(size.height / 250))
    const nextLabels = [...Array(length)].map((n, i) => ({
      y:
        (graph.bounds.maxY - graph.bounds.minY) * (i / (length - 1)) +
        graph.bounds.minY,
      top: size.height - size.height * (i / (length - 1)),
    }))

    setLabels(nextLabels)
  }

  useEffect(() => {
    refresh()
  }, [size])

  useEffect(() => {
    refresh()
  }, [graphContext.points])

  return (
    <>
      {labels.map((label) => (
        <Number
          key={label.y}
          value={label.y}
          format="0.[00]"
          className={styles.label}
          style={{ top: label.top }}
        />
      ))}
      <Text className={styles.yLabel}>{graph.yAxis}</Text>
    </>
  )
}
