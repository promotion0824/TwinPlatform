import { useEffect, useState } from 'react'
import { useSize } from '@willow/ui'
import { useGraph } from '../GraphContext'
import Label from './Label'
import styles from './XAxis.css'

export default function XAxis() {
  const graphContext = useGraph()
  const [labels, setLabels] = useState([])

  const size = useSize(graphContext.contentRef)
  const { bounds } = graphContext.graphs[0]

  function refresh() {
    if (graphContext.contentRef.current?.childNodes?.[0] == null) {
      return
    }

    const rect = graphContext.contentRef.current.getBoundingClientRect()
    const childRect =
      graphContext.contentRef.current.childNodes[0].getBoundingClientRect()
    const x = childRect.x - rect.x
    const width = rect.width - (rect.right - childRect.right)

    const length = Math.max(2, 1 + 2 * Math.floor(childRect.width / 300))

    setLabels(
      [...Array(length)].map((n, i) => ({
        x: (width - x) * (i / (length - 1)) + x,
        value: (bounds.maxX - bounds.minX) * (i / (length - 1)) + bounds.minX,
      }))
    )
  }

  useEffect(() => {
    refresh()
  }, [graphContext.contentRef, size])

  useEffect(() => {
    refresh()
  }, [graphContext.points])

  return (
    <div className={styles.xAxis}>
      {labels.map((label) => (
        <Label key={label.x} label={label} />
      ))}
    </div>
  )
}
