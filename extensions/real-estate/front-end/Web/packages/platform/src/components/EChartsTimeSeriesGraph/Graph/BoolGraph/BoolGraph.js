import { useLayoutEffect, useRef, useState } from 'react'
import { useTimeSeriesGraph } from '../../TimeSeriesGraphContext'
import styles from './BoolGraph.css'

const getOpacity = (onCount, offCount) => {
  const total = onCount + offCount

  if (!total || Number.isNaN(total)) return 0

  const percentOn = (onCount / total) * 100

  if (percentOn >= 75) {
    return 1
  } else if (percentOn >= 50) {
    return 0.75
  } else if (percentOn >= 25) {
    return 0.5
  } else {
    return 0.25
  }
}

/**
 * Draw a boolean graph over a given time period (x-axis).
 * This is a horizontal line where the brightness is based on the
 * percentage when twin has an "on" record.
 * @see getOpacity
 *
 * Rough type for `graph`:
 *
 * {
 *   lines: [ // Single-element array
 *     {
 *       color: string,
 *       data: Array<{
 *         onCount: number,
 *         offCount: number,
 *         time: number,
 *       }>
 *     }
 *   ]
 * }
 *
 * "on" means that the `onCount` for the data point is greater than zero.
 * The color of this graph is based on point's color and graph is drawn
 * when data is present (i.e. when there are at least one on and/or off count).
 */
export default function BoolGraph({ graph }) {
  const timeSeriesGraph = useTimeSeriesGraph()

  const svgRef = useRef()
  const [rects, setRects] = useState([])

  const line = graph.lines[0]

  function getBounds() {
    return {
      minX: new Date(timeSeriesGraph.times[0]).valueOf(),
      maxX: new Date(timeSeriesGraph.times[1]).valueOf(),
    }
  }

  const bounds = getBounds()

  function refresh() {
    const svgHeight = svgRef.current.clientHeight
    const svgWidth = svgRef.current.clientWidth

    function getX(time) {
      const percentage = (time - bounds.minX) / (bounds.maxX - bounds.minX)

      return percentage * svgWidth
    }

    // We generate a series of SVG rects for data with at least one onCount and/or
    // offCount. Each rect consists of x1, x2 (x-coordinate of start and end point),
    // height and opacity.
    // Note: The rect with same opacity are grouped together as a single rect because
    // rendering multiple rects/paths are placed side by side causes small gap in
    // between [even when stroke-width is 0]
    const newRects = []
    const hasNoData = (point) => !point.onCount && !point.offCount

    let i = 0
    while (i < line.data.length - 1) {
      const point = line.data[i]

      if (hasNoData(point)) {
        // Increment the index and do nothing.
        i += 1
      } else {
        const opacity = getOpacity(point.onCount, point.offCount)

        // Find the index of next point which either has no data or has different opacity.
        let nextIndex = i + 1
        while (nextIndex < line.data.length - 1) {
          const nextPoint = line.data[nextIndex]
          if (
            hasNoData(nextPoint) ||
            opacity !== getOpacity(nextPoint.onCount, nextPoint.offCount)
          ) {
            // Found the nextPoint, break out of this loop
            break
          }

          nextIndex += 1
        }

        newRects.push({
          x1: getX(point.time),
          x2: getX(line.data[nextIndex].time),
          opacity,
          height: svgHeight,
        })

        i = nextIndex
      }
    }

    setRects(newRects)
  }

  useLayoutEffect(() => {
    refresh()
  }, [line, timeSeriesGraph.size])

  return (
    <>
      <svg
        ref={svgRef}
        className={styles.svg}
        data-graph
        fill={line.color}
        strokeWidth={0}
      >
        <g>
          {rects?.map((rect) => (
            <rect
              key={rect.x1}
              x={rect.x1}
              width={rect.x2 - rect.x1}
              height={rect.height}
              opacity={rect.opacity}
            />
          ))}
        </g>
      </svg>
      <div className={styles.animatingCover} />
    </>
  )
}
