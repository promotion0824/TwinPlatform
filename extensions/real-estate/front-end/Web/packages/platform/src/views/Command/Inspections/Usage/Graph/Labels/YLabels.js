import { useEffect, useState } from 'react'
import { useWindowEventListener, Number } from '@willow/ui'
import { useGraph } from '../GraphContext'
import styles from './Labels.css'

export default function YLabels() {
  const graph = useGraph()

  const [size, setSize] = useState(2)

  function refresh() {
    let nextSize = Math.floor(graph.svgRef.current.clientHeight / 100)
    if (nextSize % 2 === 0) {
      nextSize -= 1
    }
    nextSize = Math.max(nextSize, 0) + 2

    setSize(nextSize)
  }

  useEffect(() => {
    refresh()
  }, [])

  useWindowEventListener('resize', refresh)

  const values = Array.from(Array(size)).map((n, i) => ({
    value: ((size - 1 - i) / (size - 1)) * graph.maxValue,
    top: `${(i / (size - 1)) * 100}%`,
  }))

  return (
    <>
      {values.map((value, i) => (
        <span
          key={i} // eslint-disable-line
          className={styles.yLabel}
          style={{ top: value.top }}
        >
          <Number value={value.value} format="0" />
        </span>
      ))}
    </>
  )
}
