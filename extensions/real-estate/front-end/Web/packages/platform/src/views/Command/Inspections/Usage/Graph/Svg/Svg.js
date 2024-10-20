import { useState } from 'react'
import { useGraph } from '../GraphContext'
import Tooltip from '../Tooltip/Tooltip'
import styles from './Svg.css'

export default function Svg() {
  const graph = useGraph()

  const [selected, setSelected] = useState()

  return (
    <>
      <svg ref={graph.svgRef} className={styles.svg}>
        {graph.columns.map((column, i) => (
          <g
            key={i} // eslint-disable-line
            style={{
              animation: `svg-enter 0.2s ${
                i * (0.2 / graph.columns.length)
              }s ease forwards`,
            }}
          >
            {column.segments.map((segment, j) => (
              <rect
                key={j} // eslint-disable-line
                x={column.x}
                y={segment.y}
                width={column.width}
                height={segment.height}
                fill={segment.color}
                onMouseEnter={() => setSelected({ column, segment })}
                onMouseLeave={() => setSelected()}
              />
            ))}
          </g>
        ))}
        {selected != null && (
          <>
            <rect
              key={JSON.stringify(selected)}
              x={selected.column.x}
              y={selected.segment.y}
              width={selected.column.width}
              height={selected.segment.height}
              className={styles.selectedSegment}
            />
          </>
        )}
      </svg>
      {selected != null && (
        <Tooltip key={JSON.stringify(selected)} selected={selected} />
      )}
    </>
  )
}
