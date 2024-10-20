import { useLayoutEffect, useRef, useState } from 'react'
import cx from 'classnames'
import { useWindowEventListener } from '@willow/ui'
import { GraphContext } from './GraphContext'
import Labels from './Labels/Labels'
import Legend from './Legend/Legend'
import Svg from './Svg/Svg'
import colors from './colors.json'
import styles from './Graph.css'

export default function Graph({ data, days, className }) {
  const svgRef = useRef()

  const [columns, setColumns] = useState([])

  const names = [
    ...new Set(
      data.flatMap((column) =>
        column.segments.flatMap((segment) => segment.name)
      )
    ),
  ]

  function getColor(name) {
    return colors[names.indexOf(name)] ?? colors.slice(-1)[0]
  }

  const maxValue = Math.max(
    ...data.map((column) =>
      column.segments
        .flatMap((segment) => segment.value)
        .reduce((count, prev) => count + prev, 0)
    )
  )

  function refresh() {
    const padding = 3
    const columnWidth = svgRef.current.clientWidth / data.length
    const barWidth = Math.min(columnWidth - padding, 30)

    const nextColumns = data.map((column, i) => ({
      name: column.name,
      x: columnWidth * i + columnWidth / 2 - barWidth / 2,
      width: barWidth,
      left: columnWidth * i + columnWidth / 2,
      segments: column.segments.reduce((segments, segment) => {
        const prev = segments.slice(-1)[0]

        return [
          ...segments,
          {
            name: segment.name,
            value: segment.value,
            y: (prev?.y ?? 0) + (prev?.height ?? 0),
            height: (segment.value / maxValue) * svgRef.current.clientHeight,
            color: getColor(segment.name),
          },
        ]
      }, []),
    }))

    setColumns(nextColumns)
  }

  useLayoutEffect(() => {
    refresh()
  }, [])

  useWindowEventListener('resize', refresh)

  const context = {
    svgRef,
    columns,
    maxValue,
    names,
    days,
    getColor,
  }

  const cxClassName = cx(styles.graph, className)

  return (
    <GraphContext.Provider value={context}>
      <div className={cxClassName}>
        <Legend />
        <div className={styles.content}>
          <Svg />
          <Labels />
        </div>
      </div>
    </GraphContext.Provider>
  )
}
