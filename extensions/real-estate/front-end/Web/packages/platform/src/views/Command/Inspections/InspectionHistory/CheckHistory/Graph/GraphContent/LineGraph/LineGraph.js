import { useMemo, useRef } from 'react'
import cx from 'classnames'
import YAxis from './YAxis/YAxis'
import styles from './LineGraph.css'

export default function LineGraph({ x, graph, isDimmed }) {
  const graphRef = useRef()

  const linesWithPaths = useMemo(() => {
    const hasMinAndMax =
      graph.lines[0]?.points[0]?.min != null &&
      graph.lines[0]?.points[0]?.max != null

    return graph.lines.map((line) => {
      const maxPath = `M${line.points
        .map((point) => `${point.x},${point.max}`)
        .join(' ')}`
      const minPath = `M${line.points
        .map((point) => `${point.x},${point.min}`)
        .join(' ')}`
      const reversedMinPath = line.points
        .map((point) => `${point.x},${point.min}`)
        .reverse()
        .join(' ')

      const fillPath = `${maxPath} ${reversedMinPath}`

      const path = `M${line.points
        .map((point) => `${point.x},${point.y}`)
        .join(' ')}`

      return {
        ...line,
        hasMinAndMax,
        fillPath,
        maxPath,
        minPath,
        path,
      }
    })
  }, [graph.lines])

  const cxClassName = cx(styles.lineGraph, {
    [styles.isDimmed]: isDimmed,
  })

  return (
    <div ref={graphRef} className={cxClassName} data-graph>
      <div className={styles.content}>
        <svg
          viewBox="0 0 1 1"
          preserveAspectRatio="none"
          className={styles.svg}
        >
          {linesWithPaths
            .filter((line) => line.points.length > 0)
            .map((line) => (
              <>
                {line.points.map((point) => (
                  <circle
                    cx={point.x}
                    cy={point.y}
                    r={0.01}
                    fill={line.color}
                  />
                ))}
                <g key={line.pointId}>
                  {line.hasMinAndMax && (
                    <>
                      <path
                        d={line.fillPath}
                        fill={line.color}
                        className={styles.fillPath}
                      />
                      <path
                        d={line.maxPath}
                        stroke={line.color}
                        className={styles.fillPathBorder}
                      />
                      <path
                        d={line.minPath}
                        stroke={line.color}
                        className={styles.fillPathBorder}
                      />
                    </>
                  )}
                  <path
                    d={line.path}
                    stroke={line.color}
                    className={styles.path}
                  />
                </g>
              </>
            ))}
        </svg>

        <YAxis graphRef={graphRef} graph={graph} />
        {x != null &&
          graph.lines.map((line) => {
            const point = line.points.find((nextPoint) => nextPoint.x === x)
            if (point == null) {
              return null
            }

            return (
              <div
                key={line.pointId}
                className={styles.point}
                style={{
                  backgroundColor: line.color,
                  left: `${point.x * 100}%`,
                  top: `${point.y * 100}%`,
                }}
              />
            )
          })}
      </div>
    </div>
  )
}
