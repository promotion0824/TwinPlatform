import { useLayoutEffect, useRef, useState } from 'react'
import { Progress } from '@willow/ui'
import Labels from './Labels'
import Path from './Path'
import styles from './LineGraph.css'

const graphMinY = 0.01

/**
Roughly:

interface Line {
  color: string,
  data: Array<{
    average: number,
    minimum: number,
    maximum: number,
    time: number,
  }>
}


type Props = {
  graph: {
    lines: Line[],
    yAxis: string // y axis label
  },
  highlightPointTime?: number,
  granularity: number, // milliseconds
  times: [string, string],
  isLineLoading?: (line: Line) => boolean,
}

function LineGraph(props: Props) { }
*/

export default function LineGraph({
  graph,
  highlightPointTime,
  granularity,
  times,
  isLineLoading,
  size,
}) {
  const svgRef = useRef()
  const [state, setState] = useState({
    lines: [],
  })

  const bounds = getBounds(graph, times)

  function calculateX(item) {
    const width = svgRef.current.clientWidth

    return ((item.time - bounds.minX) / (bounds.maxX - bounds.minX)) * width
  }

  function calculateY(value) {
    const height = svgRef.current.clientHeight

    if (bounds.minY !== bounds.maxY) {
      if (value !== bounds.minY) {
        return ((value - bounds.minY) / (bounds.maxY - bounds.minY)) * height
      }

      return graphMinY
    }

    return height / 2
  }

  function splitByGraphSegments(data) {
    const retVal = []
    const splitsByIndex = []
    for (let i = 1; i < data.length; i++) {
      const prevPoint = data[i - 1]
      const currentPoint = data[i]

      if (prevPoint.time + granularity !== currentPoint.time) {
        splitsByIndex.push(i)
      }
    }
    if (splitsByIndex.length === 0) {
      retVal.push(data)
    } else {
      for (let i = 0; i < splitsByIndex.length; i++) {
        const index = splitsByIndex[i]

        let start
        let end
        if (i === 0) {
          start = 0
          end = index
        } else {
          start = splitsByIndex[i - 1]
          end = index
        }
        retVal.push(data.slice(start, end))

        if (i === splitsByIndex.length - 1) {
          retVal.push(data.slice(index, data.length))
        }
      }
    }

    return retVal
  }

  function refresh() {
    const nextLines = graph.lines
      .filter((line) => line.data.length > 0)
      .map((line) => {
        if (isLineLoading?.(line)) {
          return [
            {
              isLoading: true,
            },
          ]
        }

        // Split into segments based on graph gaps
        const retVal = []
        const segments = splitByGraphSegments(line.data)

        for (const segment of segments) {
          const isSingleDotLine = segment.length === 1

          let path = 'M'
          if (isSingleDotLine) {
            const item = segment[0]
            const x = calculateX(item)
            const y = calculateY(item.average)
            const R = 3
            const offset = item.average === 0 ? R : -R

            path += `
            ${x - R}, ${y + offset}
            a ${R},${R} 0 1,1 ${R * 2},0
            a ${R},${R} 0 1,1 -${R * 2},0
          `
          } else {
            path += segment
              .map((item) => {
                const x = calculateX(item)
                const y = calculateY(item.average)

                return `${x},${y}`
              })
              .join(' ')
          }

          const maximumPath = segment
            .map((item) => {
              const x = calculateX(item)
              const y = calculateY(item.maximum)

              return `${x},${y}`
            })
            .join(' ')

          const minimumPath = segment
            .map((item) => {
              const x = calculateX(item)
              const y = calculateY(item.minimum)

              return `${x},${y}`
            })
            .reverse()
            .join(' ')

          const areaPath = `M${maximumPath} ${minimumPath}Z`
          const maxPath = `M${maximumPath}`
          const minPath = `M${minimumPath}`

          retVal.push({
            ...line,
            color: line.color,
            bounds,
            isSingleDotLine,
            path,
            areaPath,
            maxPath,
            minPath,
          })
        }

        return retVal
      })

    setState({
      lines: nextLines,
    })
  }

  useLayoutEffect(() => {
    refresh()
  }, [graph.lines, size])

  return (
    <>
      <svg ref={svgRef} className={styles.svg} data-graph>
        {state.lines.map((line, i) =>
          line.map((segment, j) => {
            if (segment.isLoading) {
              return (
                <foreignObject
                  key={segment}
                  x={svgRef.current.clientWidth / 2 - 25}
                  y={svgRef.current.clientHeight / 2 - 25}
                  width="50"
                  height="50"
                >
                  <Progress />
                </foreignObject>
              )
            }

            return (
              <g
                key={`${i}${j}`} // eslint-disable-line
              >
                <path
                  d={segment.areaPath}
                  fill={segment.color}
                  opacity={0.1}
                  className={styles.areaPath}
                />
                <path
                  d={segment.maxPath}
                  stroke={segment.color}
                  fill="none"
                  opacity={0.15}
                  className={styles.areaPath}
                />
                <path
                  d={segment.minPath}
                  stroke={segment.color}
                  fill="none"
                  opacity={0.15}
                  className={styles.areaPath}
                />
                <Path
                  d={segment.path}
                  stroke={segment.color}
                  fill={segment.isSingleDotLine ? segment.color : 'none'}
                />
              </g>
            )
          })
        )}
      </svg>

      <Labels
        svgRef={svgRef}
        graph={{ unit: graph.yAxis, ...graph }}
        bounds={bounds}
      />

      {highlightPointTime != null &&
        state.lines.map((line) => {
          let point = null
          for (const lineSegment of line) {
            const nextPoint = lineSegment.data?.find(
              (data) => data.time === highlightPointTime
            )
            if (nextPoint) {
              point = [nextPoint, lineSegment]
              break
            }
          }

          if (point == null) {
            return null
          }

          const [searchPoint, segment] = point
          const x = calculateX(searchPoint)
          const y = calculateY(searchPoint.average)

          return (
            <div
              key={segment.pointId}
              className={styles.point}
              style={{
                backgroundColor: segment.color,
                left: `${x}px`,
                bottom: `${y - 12}px`,
                top: 'auto',
              }}
            />
          )
        })}
    </>
  )
}

function getBounds(graph, times) {
  const yValues = graph.lines
    .flatMap((line) =>
      line.data.flatMap((item) => [item.average, item.maximum, item.minimum])
    )
    .filter((x) => typeof x === 'number')

  return {
    minX: new Date(times[0]).valueOf(),
    maxX: new Date(times[1]).valueOf(),
    minY: yValues.reduce((min, y) => (y < min ? y : min), Infinity),
    maxY: yValues.reduce((max, y) => (y > max ? y : max), -Infinity),
  }
}
