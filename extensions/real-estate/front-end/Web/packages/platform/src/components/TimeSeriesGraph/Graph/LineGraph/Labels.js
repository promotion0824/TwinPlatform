import { useLayoutEffect, useState } from 'react'
import { Number } from '@willow/ui'
import styles from './Labels.css'

export default function Labels({ svgRef, graph, bounds }) {
  const [labels, setLabels] = useState([])

  useLayoutEffect(() => {
    const height = svgRef.current.clientHeight

    const min = bounds.minY
    const max = bounds.maxY

    let count = 3
    if (height > 600) count = 5

    const nextLabels = Array.from(Array(count)).map((n, i) => {
      const percentage = i / (count - 1)

      return {
        top: height * percentage,
        value: (max - min) * (1 - percentage) + min,
      }
    })

    setLabels(nextLabels)
  }, [svgRef.current?.clientHeight, JSON.stringify(bounds)])

  return (
    <>
      {labels.map((label, i) => (
        <div
          key={i} // eslint-disable-line
          className={styles.label}
          style={{
            top: label.top,
          }}
        >
          <Number value={label.value} format="0.00" />
        </div>
      ))}
      <div className={styles.yLabel}>{graph.unit}</div>
    </>
  )
}
