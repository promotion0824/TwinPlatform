import { useLayoutEffect, useRef, useState } from 'react'
import { styled } from 'twin.macro'
import { useTimeSeriesGraph } from '../../TimeSeriesGraphContext'

const getOpacity = (valueMap, state, opacityTable) => {
  if (!state) {
    return 0
  }

  const maxStateKey = Object.keys(state).reduce((a, b) =>
    state[a] > state[b] ? a : b
  )

  const maxStateValue = valueMap[maxStateKey]
  const opacity = opacityTable[maxStateValue]

  return opacity
}

const Svg = styled.svg({
  borderLeft: '1px solid transparent',
  borderRight: '1px solid transparent',
  flex: '1',
  height: '34px',
  transform: 'scaleY(-1)',
  width: '100%',
})

/**
 * Draw a horizontal graph over a given time period (x-axis).
 * This is a horizontal line where the number of state determines the opacity of the rect
 * and based on the alphabetical position of the states.
 * @see getOpacity
 *
 * Rough type for `graph`:
 *
 * {
 *   lines: [ // Single-element array
 *     {
 *       color: string,
 *       data: Array<{
 *         state: Record<string, number>,
 *         time: number,
 *       }>
 *     }
 *   ]
 * }
 *
 * State contains aggregate number of a single states in a timeframe.
 * The color of this graph is based on state's alphabetical position from
 * dimmest to brightest.
 */
export default function MultiStateGraph({ graph }) {
  const timeSeriesGraph = useTimeSeriesGraph()

  const svgRef = useRef()
  const [rects, setRects] = useState([])

  const line = graph.lines[0]
  const { valueMap } = line
  const orderValueMap = Object.values(valueMap).sort()
  const opacityTable = Object.fromEntries(
    orderValueMap.map((x, z) => [x, (z + 1) / Object.values(valueMap).length])
  )

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

    // We generate a series of SVG rects for states.
    // Each rect consists of x1, x2 (x-coordinate of start and end point),
    // height and opacity.
    // Note: The rect with same opacity are grouped together as a single rect because
    // rendering multiple rects/paths are placed side by side causes small gap in
    // between [even when stroke-width is 0]

    const newRects = []
    const hasNoData = (point) => Object.keys(point.state).length === 0

    let i = 0
    while (i < line.data.length - 1) {
      const point = line.data[i]

      if (hasNoData(point)) {
        // Increment the index and do nothing.
        i += 1
        continue
      }
      const opacity = getOpacity(valueMap, point.state, opacityTable)

      // Find the index of next point which either has no data or has different opacity.
      let nextIndex = i + 1
      while (nextIndex < line.data.length - 1) {
        const nextPoint = line.data[nextIndex]
        const nextPointOpacity = getOpacity(
          valueMap,
          nextPoint.state,
          opacityTable
        )
        if (hasNoData(nextPoint) || opacity !== nextPointOpacity) {
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

    setRects(newRects)
  }

  useLayoutEffect(() => {
    refresh()
  }, [line, timeSeriesGraph.size])

  return (
    <Svg ref={svgRef} data-graph fill={line.color} strokeWidth={0}>
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
    </Svg>
  )
}
